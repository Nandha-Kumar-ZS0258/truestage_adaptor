-- ============================================================
-- TrueStage POC — Target SQL Schema
-- Built once. Never changes. Config drives CU differences.
-- ============================================================

-- CU Registry — master list of all onboarded Credit Unions
CREATE TABLE CU_Registry (
    cu_id           VARCHAR(50)  PRIMARY KEY,
    cu_name         VARCHAR(200) NOT NULL,
    status          VARCHAR(20)  DEFAULT 'ONBOARDING',  -- ONBOARDING / ACTIVE / INACTIVE
    adapter_version VARCHAR(10),
    config_path     VARCHAR(500),                        -- blob path to JSON mapping config
    sla_hours       INT          DEFAULT 24,
    onboarded_date  DATETIME     DEFAULT GETUTCDATE()
);

-- Members — canonical member table (all CUs write here)
-- Schema derived from CCCU_member_details.csv
CREATE TABLE Members (
    id               BIGINT        IDENTITY PRIMARY KEY,
    cu_id            VARCHAR(50)   NOT NULL REFERENCES CU_Registry(cu_id),
    entity_uuid      VARCHAR(100),                       -- source UUID from CU system
    member_id        VARCHAR(100)  NOT NULL,             -- source member identifier
    member_name      VARCHAR(200),                       -- full name (first + last combined)
    birth_date       DATE,
    street_name      VARCHAR(500),
    building_number  VARCHAR(50),
    postal_code      VARCHAR(20),
    town_name        VARCHAR(100),
    state            VARCHAR(10),
    country          VARCHAR(10),
    tax_id_masked    VARCHAR(50),                        -- masked SSN / tax ID
    email            VARCHAR(255),
    phone_number     VARCHAR(50),
    as_of_date       DATE,
    record_hash      VARCHAR(64),                        -- SHA256 for change detection
    is_current       BIT           DEFAULT 1,
    ingested_at      DATETIME      DEFAULT GETUTCDATE(),
    adapter_version  VARCHAR(10),
    source_file      VARCHAR(500),
    UNIQUE (cu_id, member_id)
);

-- LoanAccounts — loan and account data per member
-- Schema derived from CCCU_loan_account_details.csv
CREATE TABLE LoanAccounts (
    id                     BIGINT        IDENTITY PRIMARY KEY,
    cu_id                  VARCHAR(50)   NOT NULL REFERENCES CU_Registry(cu_id),
    loan_uuid              VARCHAR(100),                 -- source loan UUID
    account_number         VARCHAR(100)  NOT NULL,       -- source account number
    account_type           VARCHAR(50),                  -- mortgage / personal / auto / etc.
    account_status         VARCHAR(50),                  -- active / closed / delinquent / etc.
    product_id             VARCHAR(100),
    entity_uuid            VARCHAR(100),                 -- FK reference to Members.entity_uuid
    relationship_type_id   VARCHAR(100),
    open_date              DATE,
    interest_rate          DECIMAL(10,4),
    billing_cycle_day      INT,
    term_in_months         INT,
    original_amount        DECIMAL(18,2),
    currency               VARCHAR(10),
    collateral_description VARCHAR(MAX),
    record_hash            VARCHAR(64),                  -- SHA256 for change detection
    is_current             BIT           DEFAULT 1,
    ingested_at            DATETIME      DEFAULT GETUTCDATE(),
    adapter_version        VARCHAR(10),
    source_file            VARCHAR(500),
    UNIQUE (cu_id, account_number)
);

-- Ingestion Log — audit trail per file processed
CREATE TABLE Ingestion_Log (
    job_id         VARCHAR(100) PRIMARY KEY,
    cu_id          VARCHAR(50)  NOT NULL,
    source_file    VARCHAR(500) NOT NULL,
    status         VARCHAR(20)  NOT NULL,               -- IN_PROGRESS / COMPLETED / FAILED
    total_rows     INT          DEFAULT 0,
    success_rows   INT          DEFAULT 0,
    failed_rows    INT          DEFAULT 0,
    rows_new       INT          DEFAULT 0,
    rows_updated   INT          DEFAULT 0,
    rows_unchanged INT          DEFAULT 0,
    sla_hours      INT,
    sla_met        BIT,
    started_at     DATETIME     DEFAULT GETUTCDATE(),
    completed_at   DATETIME,
    error_summary  VARCHAR(MAX)
);

-- Row Error Log — captures raw data of failed rows for investigation
CREATE TABLE Row_Error_Log (
    error_id      BIGINT       IDENTITY PRIMARY KEY,
    job_id        VARCHAR(100) REFERENCES Ingestion_Log(job_id),
    cu_id         VARCHAR(50),
    row_number    INT,
    raw_data      VARCHAR(MAX),
    error_message VARCHAR(MAX),
    occurred_at   DATETIME     DEFAULT GETUTCDATE()
);

-- Indexes for query performance
CREATE INDEX IX_Members_CuId ON Members(cu_id);
CREATE INDEX IX_Members_EntityUuid ON Members(entity_uuid);
CREATE INDEX IX_LoanAccounts_CuId ON LoanAccounts(cu_id);
CREATE INDEX IX_LoanAccounts_EntityUuid ON LoanAccounts(entity_uuid);
CREATE INDEX IX_Ingestion_Log_CuId ON Ingestion_Log(cu_id);
CREATE INDEX IX_Row_Error_Log_JobId ON Row_Error_Log(job_id);

-- Seed: Register sample CUs for POC
INSERT INTO CU_Registry (cu_id, cu_name, status, config_path) VALUES
    ('CU_ALPHA',           'Alpha Community Credit Union',   'ACTIVE',     'configs/cu_alpha_mapping.json'),
    ('CU_BETA',            'Beta Federal Credit Union',      'ACTIVE',     'configs/cu_beta_mapping.json'),
    ('CCCU_MEMBER_DETAILS','Coastal Community Credit Union', 'ONBOARDING', 'configs/cccu_member_details_mapping.json');
