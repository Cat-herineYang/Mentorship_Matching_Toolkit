# Mentor–Mentee Matching System

This README guides you through setting up and running the Mentor–Mentee Matching System from start to finish. No prior experience with Qualtrics, CSV files, or coding is required.

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Step 1: Design & Distribute Surveys](#step-1-design--distribute-surveys)
4. [Step 2: Export & Clean CSV Data](#step-2-export--clean-csv-data)
5. [Step 3: Run Matching Application Locally](#step-3-run-matching-application-locally)
6. [Step 4: Generate & Download Pairing Results](#step-4-generate--download-pairing-results)
7. [Troubleshooting](#troubleshooting)
8. [Contact & Resources](#contact--resources)

---

## Overview

This project automates pairing mentors and mentees based on survey responses. It consists of three main phases:

1. **Survey Creation & Distribution** (Qualtrics)
2. **Data Export & Cleaning** (Google Sheets & CSV Formatter)
3. **Local Matching Application** (Web interface)

By following these steps, staff can efficiently collect preferences, clean data, and produce a pairing list with minimal manual effort.

---

## Prerequisites

Before you begin, make sure you have:

* **Internet access** to use Qualtrics and the CSV formatting website.
* A computer with a **modern web browser** (Chrome, Firefox, Edge).
* **Git** installed (to clone the project repository).
* **Node.js & npm** installed (to run the local web application).

If you need help installing Git or Node.js, see:

* Git: [https://git-scm.com/](https://git-scm.com/)
* Node.js: [https://nodejs.org/](https://nodejs.org/)

---

## Step 1: Design & Distribute Surveys

1. **Create Qualtrics Account**

   * Go to [https://www.qualtrics.com](https://www.qualtrics.com) and sign up or log in.

2. **Import Survey Template**

   * Download the example survey from:
     `https://catryang.com/surveytemplate`
   * In Qualtrics, go to **Projects → Create Project → Survey → Upload a QSF file** and select the downloaded template.

3. **Customize Questions**

   * Add any organization-specific questions if needed (e.g., campus location, year).
   * Ensure key fields are present: **Name**, **Email**, **Major**, **Location**, etc.

4. **Set Distribution & Deadline**

   * Under **Distributions**, choose email or anonymous link.
   * Set a **closing date/time** for responses (e.g., two weeks from launch).

5. **Launch Survey**

   * Send the survey link to your mentor and mentee lists.
   * Monitor responses under **Data & Analysis**.

---

## Step 2: Export & Clean CSV Data

1. **Export Responses**

   * In Qualtrics, navigate to **Data & Analysis → Export & Import → Export Data**.
   * Choose **CSV** format and download as: `mentee_info.csv` (and `mentor_info.csv` for mentor survey).

2. **Rename & Standardize Columns**

   * Open the CSV file in **Google Sheets**.
   * Ensure the header row uses clear column names:

     * **Name**, **Email**, **Major**, **Location**, **Goal**, etc.
   * If a header is missing or unclear, click the cell and type the correct name.

3. **Download as CSV**

   * In Google Sheets, go to **File → Download → Comma-separated values (.csv)**.

4. **Clean with CSV Formatter**

   * Visit: `https://catryang.com/csvformat`
   * Click **Upload** and select your `mentee_info.csv` (and repeat for mentors).
   * The tool will:

     * Keep required columns (Name, Email) and any optional columns you’ve included.
     * Add a `NumStudent` column set to 0.
     * Validate names (no digits), emails (non‑empty), and detect duplicates.
   * If errors appear, fix them in your spreadsheet and re‑upload.
   * Once no errors remain, click **Download Cleaned CSV** to save `mentee_info_cleaned.csv`.

---

## Step 3: Run Matching Application Locally

1. **Clone Project Repository**

   ```bash
   git clone https://github.com/YourUsername/mentor-mentee-matching.git
   cd mentor-mentee-matching
   ```

2. **Install Dependencies**

   ```bash
   npm install
   ```

3. **Start the Server**

   ```bash
   npm start
   ```

4. **Open in Browser**

   * Go to `http://localhost:4330/`
   * You should see the matching interface with two upload buttons.

---

## Step 4: Generate & Download Pairing Results

1. **Upload Cleaned CSVs**

   * Click the **Upload** button for mentors and select `mentor_info_cleaned.csv`.
   * Click the **Upload** button for mentees and select `mentee_info_cleaned.csv`.

2. **Run Matching Algorithm**

   * After both files are uploaded, the system will automatically process the data.
   * Processing time is fast (under 5 seconds for \~200 pairs).

3. **Download Results**

   * Once complete, a **Download Pairings** button appears.
   * Click it to save `pairing_results.csv`, which lists matched mentor–mentee pairs.

---

## Troubleshooting

* **Qualtrics Export Issues**: Ensure you selected **CSV**, not XLSX.
* **Column Name Mismatches**: Confirm header names exactly match (case‑sensitive).
* **Errors in CSV Formatter**: Fix reported issues in your spreadsheet, re‑export, and re‑upload.
* **Server Won’t Start**:

  * Verify Node.js is installed: `node -v` and `npm -v`.
  * Check port 4330 is not in use.

---

## Contact & Resources

* **Survey Template**: [https://catryang.com/surveytemplate](https://catryang.com/surveytemplate)
* **CSV Formatter**: [https://catryang.com/csvformat](https://catryang.com/csvformat)
* **Source Code & Issues**: [https://github.com/Cat-herineYang/Mentorship_Matching_Toolkit](https://github.com/Cat-herineYang/Mentorship_Matching_Toolkit)
* **Support**: Contact Catherine Yang at `catherine.yang@duke.edu`

Thank you for using this matching system! Feel free to reach out with any questions or feedback.
