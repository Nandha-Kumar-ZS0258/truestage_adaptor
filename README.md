Invoke with	What it does
/onboard-cu ./samples/cu_gamma_sample.csv CU_GAMMA	Full CU onboarding — maps columns, generates config + tests, runs tests
/validate-mapping CU_ALPHA	Validates a config against target schema + sample file
/run-regression	Builds + runs all tests, auto-fixes failures
/reprocess-cu CU_ALPHA ./samples/cu_alpha_sample.csv	Dry-run preview then reprocess a file
