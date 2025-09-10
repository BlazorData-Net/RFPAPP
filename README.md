# RFP Response Creator  

![License](https://img.shields.io/badge/license-MIT-blue.svg)  ![Tech](https://img.shields.io/badge/Blazor-.NET%209-blueviolet) ![AI Powered](https://img.shields.io/badge/AI-OpenAI-success)  

🚀 **RFP Response Creator** is a SaaS application that automates professional Request for Proposal (RFP) responses.  
Using AI, it extracts questions, generates answers from your knowledge base, and produces polished Word/PDF documents.  
For venues, it includes a **smart scheduling engine** to assign rooms without conflicts.  

### Online Live version:
[https://RFP.BlazorData.net](https://RFP.BlazorData.net)

---

## ✨ Core Purpose
Save time and win more proposals by automating RFP responses:
- Detects questions in RFPs  
- Drafts answers using your uploaded materials  
- Assigns rooms intelligently (if using the optional venue mode)  
- Exports professional, ready-to-send documents  

---

## 🧩 Key Features  

### 1. AI-Powered Document Processing
- OCR integration for PDFs, Word, and images  
- Multi-format support: `.docx`, `.pdf`, `.png`, `.jpg`, `.jpeg`  
- Automatic text extraction for scanned docs  

### 2. Intelligent Question Detection & Answering
- Finds questions automatically in RFPs  
- Embeddings-based semantic search against your knowledge base  
- AI-generated draft responses with manual editing  

### 3. Venue Management System
- Upload & process **capacity charts** with AI  
- Auto room assignment based on capacity, setup, and availability  
- Prevents double-booking with conflict detection  
- Supports **10 setups**: Banquet, Conference, Theatre, Classroom, U-Shape, Boardroom, Hollow Square, Crescent Rounds, Reception, Square  
- Smart re-calculation (respects existing assignments)  

### 4. Template System
- Built-in **Default** and **DefaultVenue** templates  
- Custom templates with dynamic tokens (e.g., `[BUSINESS_NAME]`, `[EVENT_NAME]`)  
- Save, edit, and organize templates for reuse  

### 5. Knowledge Base Management
- Upload documents and chunk content for semantic search  
- Vector embeddings for accurate Q&A matching  
- Token management for reusable snippets  
- Group and categorize entries  

### 6. Document Generation
- Professional `.docx` Word documents  
- Auto-generated **venue tables**  
- Export options: Word, PDF, CSV  
- Clean formatting with consistent branding  

### 7. Room Calculation Engine
- First-fit algorithm assigns smallest suitable room  
- Time conflict detection  
- Supports parent-child room grouping  

### 8. Data Management
- Save/load configurations  
- Local storage with backup/restore  
- Import/export settings packages  
- Automatic `.zip` compression for portability  

### 9. User Interface
- Guided **4-step workflow**  
- Real-time AI processing updates  
- Interactive grids for room assignments & responses  
- Fully responsive design  

### 10. Integration
- Supports **GPT-4o, GPT-5, and future OpenAI models**  
- Secure API key management  
- Logging system for debugging  

---

## 🔄 Workflow  

1. **Select Template** – Built-in or custom  
2. **Upload RFP** – Process documents with OCR  
3. **Review Q&A** – Edit AI-detected questions and answers  
4. **Assign Venues** – Smart room scheduling (venue mode)  
5. **Generate Document** – Export Word/PDF/CSV  

---

## 🎯 Target Users
- **Venue Managers** – Hotels, conference centers, event spaces  
- **Sales Teams** – Faster, more consistent responses  
- **Event Planners** – Manage scheduling and capacity with ease  
- **Business Development** – Standardize proposals and scale efforts  

---

## 📦 Tech Stack
- **Blazor .NET 9**  
- **Radzen Components** (UI)  
- **OpenAI Integration** (GPT-4o, GPT-5, etc.)  
- **Local + Cloud Data Management**  

---

## 📸 Screenshots  
<img width="829" height="729" alt="RFPResponseCreatorScreenShot" src="https://github.com/user-attachments/assets/961bae0a-8cb0-40cd-b57a-fa4894676903" />

<img width="957" height="472" alt="RFPResponseCreatorScreenShot2" src="https://github.com/user-attachments/assets/fcd252e3-d655-4209-8478-8d957bac1be7" />
---
