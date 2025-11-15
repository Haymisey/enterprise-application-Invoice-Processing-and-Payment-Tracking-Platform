# Project Topic: Invoice Processing & Payment Tracking Platform

| Full Name        | ID          |
|------------------|-------------|
| Bezawit Geta     | UGR/9103/15 |
| Haymanot Abera   | UGR/9265/15 |
| Nuhamien Tariku  | UGR/2940/15 |
| Siham Sadik      | UGR/0794/15 |
| Thitna Damtew    | UGR/1387/15 |

---

## Core Domain
**Invoice Lifecycle Management** — creation, validation, verification, and payment tracking.

Many organizations still manage invoices and payments through manual, disconnected processes, which leads to delays, errors, duplicate records, and poor financial visibility. To address these issues, there is a need for an integrated system that automates invoice processing, tracks payments accurately, and improves overall financial control.

---

## Bounded Contexts

### 1. **Invoice Management BC (Core)**
Handles the full lifecycle of an invoice:
- Creation
- Validation
- Verification
- Aging (tracking how long invoices remain unpaid)

### 2. **Payment Tracking BC**
Responsible for:
- Tracking payment status  
- Handling scheduled and completed payments  
- Generating overdue and escalation alerts  

### 3. **Vendor Management BC**
Manages vendor/supplier/contractor information:
- Vendor profiles  
- Contract details  
- Historical transaction insights  

### 4. **Reporting BC**
Financial reporting and dashboards:
- Cashflow analysis  
- Outstanding balances  
- Monthly/quarterly invoice summaries  

### 5. **AI Classification BC**
AI-powered processing:
- Auto-extract invoice data from PDFs/images  
- Duplicate invoice detection  
- Fraud pattern analysis using ML  

---

## Key Use Cases

### **1. Upload Invoice → AI Extraction**
- User uploads a document (PDF, image).  
- AI extracts fields such as sender, total amount, items, dates, tax, etc.  
- AI Classification BC publishes **`InvoiceExtracted`**.  
- **Invoice Management BC** listens and creates a draft invoice.


### **2. Approve & Schedule Payments**
- Accountant reviews and approves the invoice.  
- Invoice Management BC publishes **`InvoiceApproved`**.  
- **Payment Tracking BC** listens and schedules the payment.


### **3. Track Paid vs Unpaid Invoices**
- Payment Tracking BC completes or updates payment status.  
- It publishes **`PaymentCompleted`** or **`PaymentStatusUpdated`**.  
- **Reporting BC** listens and updates cashflow and outstanding balance reports.


### **4. Detect Duplicate/Fraudulent Invoices**
- AI analyzes incoming invoices for duplicates or suspicious patterns.  
- AI Classification BC publishes **`SuspiciousInvoiceDetected`**.  
- **Invoice Management BC** listens and marks the invoice for manual review.

