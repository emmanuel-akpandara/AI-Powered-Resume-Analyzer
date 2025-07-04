#  Resume Analyzer

AI-powered platform for intelligent job matching based on resume content, skill embeddings, and semantic relevance. This project streamlines the job discovery process by comparing user-uploaded resumes against a dynamic job vector database — surfacing the most relevant roles instantly.

---

##  What It Does

Resume Analyzer transforms traditional keyword-based job search into a smarter, personalized experience using:

-  **AI Embedding Matching**: Resumes are converted to dense vector representations using language model embeddings. These vectors are semantically compared to job descriptions using cosine similarity.
-  **Pinecone Integration**: Job postings are stored as high-dimensional vectors in Pinecone — a cloud-native vector database. Real-time similarity queries retrieve the most relevant matches.
-  **PDF Resume Upload**: Users submit resumes via a Blazor-based frontend. The backend parses and embeds the content, routing it through the match pipeline.

---

##  Technology Stack

| Layer       | Tech Used                                 | Purpose                              |
|-------------|--------------------------------------------|---------------------------------------|
| Client UI   | Blazor WebAssembly + Bootstrap 5           | Resume input + match presentation     |
| Hosting     | ASP.NET Core                               | Razor component hosting & APIs        |
| API         | .NET Web API                               | Resume ingestion + embedding logic    |
| Storage     | Pinecone vector database                   | Stores & queries job vectors          |
| Embedding   | OpenAI (text-embedding-3-small)        | Converts resume and job text to vectors |

---

##  How Matching Works

1.  **User uploads resume** (PDF format via UI)
2.  **Text is extracted** using `PdfPig (UglyToad)` 
3.  **Resume is embedded** into a vector using OpenAI’s text-embedding-3-small
4.  **Job vectors are queried in Pinecone** for similarity (cosine distance).  (Jobs were formerly scraped and stored to Pinecone with jobseeder)
5.  **Top 10 jobs are returned** with match scores and skill overlaps
6.  **Results are displayed** in dynamic cards + modal views

---

##  Real-World Impact

> "Thousands of qualified candidates are rejected every day because their resumes don’t match recruiter search keywords."

Resume Analyzer solves that by:

- **Elevating resume visibility** based on actual content, not formatting or keyword optimization
- **Accelerating discovery** of high-fit roles without endless browsing
- **Helping recruiters** screen better matches faster using vector-based precision
- **Enabling career growth** by surfacing roles users may not know they’re aligned with

---

##  Developer Notes

- Jobs can be ingested periodically via scraping, API import, or admin upload
- Embeddings are cached and stored in Pinecone once per job post (If jobs are added to the dataset, seeding can be set to true in program.cs in the api project)

---

##  Future Enhancements

-  Resume history & saved matches
-  Semantic skill filters (e.g. "Must match ≥3 core skills")
-  Feedback loop — allow users to rate match quality for retraining
-  Job clustering and career path suggestions via vector neighborhoods
