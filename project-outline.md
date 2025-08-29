## Project Outline
- Automate tenant payment processing to reduce manual reconciliation.  
- Ensure accurate matching between tenant receipts and settlement reports.  
- Improve efficiency in generating digital invoices.  
- Provide a structured storage system for receipts and reports for easy reference.  

---

# Project Flow

## Tenant Submission
- Tenant sends payment receipt via WhatsApp to management number.

## Receipt Processing
- System receives and parses receipt → extracts payment details (amount, date, payment method, reference no.).
- Match tenant phone number with master tenant list to identify the correct unit.
- Store receipt in structured folder by date: `/receipts/YYYY-MM-DD/UnitNumber/` 

## Settlement Report Handling
- Admin downloads **Merchant Daily Settlement Report** (CSV/XLS).
- Uploads/feeds report into the system.

## Reconciliation Process
- System compares receipts with settlement report (date, amount, time).  
*(This process is formally called **Payment Reconciliation**.)*
- Add status column → **Matched / Mismatched**.
- System appends a new **Status** column to the existing settlement Excel file, marking each row as **Matched** or **Mismatched**.

---

# Milestones & Deliverables

## Phase 1 – Setup & Receipt Processing
✅ Integrated WhatsApp channel via Twilio for seamless inbound messaging.  
✅ Built OCR/Parsing service to automatically extract receipt details.  
✅ Established Tenant Master List with phone-to-unit mapping.  
✅ Designed structured receipt storage system: `/receipts/YYYY-MM-DD/`.  

## Phase 2 – Reconciliation Engine
✅ Automated import of Merchant Daily Settlement Reports.  
✅ Developed Reconciliation Engine (Date, Amount, Time matching).  
✅ Implemented transaction status tracking: *Matched / Unmatched*.  

## Phase 3 – Deployment & Testing  
- Deploy system in live/test environment.  
- Monitor for edge cases and unusual scenarios.  
- Define mitigation strategies for handling discrepancies.  

## Phase 4 – Invoice Automation *(Next Phase, To Be Confirmed)*  
- Automated invoice generation for reconciled receipts.  
- Organized invoice storage by unit: `/invoices/YYYY-MM/UnitNumber/`.  
- Optional distribution of invoices via Email/WhatsApp.  
