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

## 🆕 Microsoft Aspire Job Scheduler Project

We are expanding the RFPAPP platform to include a comprehensive **Microsoft Aspire Job Scheduling solution**. This new project will transform our application into a distributed job execution platform with real-time monitoring and AI-assisted development.

### 🎯 Project Goals
- **📊 Dashboard**: Real-time monitoring of job execution status
- **⚙️ Configuration**: Management of queues, containers, AI, and storage
- **📅 Scheduler/Jobs**: Create, edit, and manage job definitions with complex schedules
- **📜 Logs**: Comprehensive job execution logging and monitoring
- **🔧 Code Management**: Online C#/Python editors with AI assistance and NuGet package management

### 🏗️ Technical Architecture
- **Microsoft Aspire**: Service orchestration and development platform
- **Azure SQL Database**: Job metadata and execution history
- **Azure Service Bus**: Message queuing for job distribution  
- **Azure Blob Storage**: Job modules, logs, and artifacts
- **Azure Container Instances**: Scalable job execution environment
- **Blazor Server**: Real-time web interface with SignalR

### 📚 Planning Documentation
Comprehensive planning documents have been created for this project:

- **[Complete TODO List](./TODO_ASPIRE_JOB_SCHEDULER.md)** - Master checklist with 200+ implementation tasks
- **[Database Schema](./Database/AspireJobSchedulerSchema.sql)** - Complete SQL schema with 8 core tables
- **[Azure Configuration Guide](./AzureServicesConfiguration.md)** - Step-by-step Azure infrastructure setup
- **[Blazor Components Spec](./BlazorComponentsSpecification.md)** - Detailed UI component specifications
- **[Implementation Roadmap](./ImplementationRoadmap.md)** - 10-week development timeline with phases
- **[Project Summary](./AspireJobSchedulerProjectSummary.md)** - Comprehensive project overview

### 🚀 Implementation Timeline
- **Phase 1** (Weeks 1-2): Foundation & Infrastructure
- **Phase 2** (Weeks 3-4): Job Scheduling Engine  
- **Phase 3** (Weeks 5-6): Web Application UI
- **Phase 4** (Weeks 7-8): AI Integration & Advanced Features
- **Phase 5** (Weeks 9-10): Security, Testing & Production

### 🔄 Job Execution Flow
1. **Module Creation**: Upload NuGet packages via Blazor UI
2. **Scheduling**: Configure recurring schedules with complex patterns
3. **Queuing**: Scheduler places job messages on Service Bus queues
4. **Execution**: Agent containers consume messages and execute jobs
5. **Monitoring**: Real-time updates displayed on dashboard
6. **Logging**: Comprehensive execution logs for debugging and auditing

This expansion represents a significant evolution of the RFPAPP platform, leveraging modern cloud-native architecture and AI capabilities to create a robust, scalable job scheduling solution.

---
