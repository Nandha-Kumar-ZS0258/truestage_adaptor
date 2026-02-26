import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "../truestage_ppt";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>
);
