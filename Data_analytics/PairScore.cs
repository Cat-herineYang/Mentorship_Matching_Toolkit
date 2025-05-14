using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Hosting;

namespace Data_analytics
{
    public class PairScore
    {
        public async Task LogAsync(string message)
        {
            await File.AppendAllTextAsync("log.txt", message + Environment.NewLine);
        }

        public int thresh = 0;
        public int Major_num = 58;
        public int Minor_num = 61;
        public int Industry_num = 44;
        public int Location_num = 21;
  
        public string[] Majors = {
        "African and African American Studies","Ancient Religion and Society","Art History",
        "Art History and Visual Arts","Asian and Middle Eastern Studies","Biology","Biomedical Engineering",
        "Biophysics","Brazilian and Global Portuguese Studies","Chemistry","Civil Engineering",
        "Classical Civilization","Classical Languages","Computational Media","Computer Science",
        "Cultural Anthropology","Dance","Earth and Climate Sciences","Economics","Electrical and Computer Engineering",
        "English","Environmental Engineering","Environmental Sciences","Environmental Sciences and Policy",
        "Evolutionary Anthropology","French Studies","Gender Sexuality and Feminist Studies","German",
        "Global Cultural Studies","Global Health","History","Interdisciplinary Engineering and Applied Science (IDEAS)",
        "International Comparative Studies","Italian and European Studies","Linguistics","Linguistics and Computer Science",
        "Marine Science and Conservation","Mathematics","Mechanical Engineering","Medieval and Renaissance Studies",
        "Music","Neuroscience","Philosophy","Physics","Political Science","Psychology","Public Policy Studies",
        "Religious Studies","Romance Studies","Russian","Slavic and Eurasian Studies","Sociology",
        "Spanish Latin American and Latinx Studies","Statistical Science","Theater Studies","Visual Arts",
        "Visual and Media Studies","Undecided","None"
        };
  
        public string[] Industries = {
        "Technology / Software / IT", "Finance / Investment / Banking", "Medical",
        "Consulting and professional services", "Healthcare", "Nursing", "Education",
        "Journalism", "Marketing / Advertising", "Arts / Creative Industries",
        "Non-Profit and Social Services", "Law / Legal Services", "Hospitality and Tourism",
        "Manufacturing and engineering", "Retail and e-commerce", "Energy and Utilities",
        "Politics / Government and public sector", "Real estate", "Construction", "Food and beverage",
        "Media Entertainment and Sports", "Transportation and logistics", "Fashion and Apparel",
        "STEM", "Artificial Intelligence / Machine Learning / Data Science",
        "Pharmaceutical", "Biotechnology", "Research and Development", "Telecommunications",
        "Aerospace and Defense", "Agricultural and Food Science", "Environmental Science",
        "Product / Product Management / UX Design", "Veterinary Medicine", "Venture Capital",
        "Accounting", "Private Equity", "Architecture and Design", "Military", "International Relations",
        "Academia / Higher Ed", "Religion", "Physical Therapy", "Dentistry","None"
        };

        public Dictionary<int, Info> mentorD;
        public Dictionary<int, Info> menteeD;
        public decimal[,] scores;
        public int columnNum = 25;

        public decimal[,] Major_Sim;
        public decimal[,] Minor_Sim;
        public decimal[,] Industry_Sim;
        public decimal[,] Location_Sim;

        public decimal avgScore;
        public decimal min;
        public decimal max;

        public List<int> mentorReId = new List<int>();
        public List<int> menteeReId = new List<int>();
        public int mentorNum;
        public int menteeNum;
        public int[] mentorID;
        public int[] menteeID;
        public int iteration;
        private int z0_row;
        private int z0_col;

        public List<string> names = new List<string>();
        public async Task<string[,]> CalculateScores(IBrowserFile mentor, IBrowserFile mentee)
        {
            // Initialization of mentor and mentee Dictionary<int,Info>
            mentorD = await SaveFile(mentor, false);
            menteeD = await SaveFile(mentee, true);
            mentorNum = mentorD.Count;
            menteeNum = menteeD.Count;

            await LogAsync("Scoring starts");
            Console.WriteLine("Scoring starts!");       
            // Get the scoring matrix
            decimal[,] scores = await GetScoreMatrix(mentorD, menteeD);
            await LogAsync("Scoring finished");
            Console.WriteLine("Scoring finished!");
            // Matches the pairs using the scoring matrix
            string[,] pairingRecords = await StartMatching(scores, 0);

            // Statistics update

            return pairingRecords;
        }
        
        // Save the file of mentor/mentee information into the Database
        // Return a Dictionary with count num being the key and Info being the value
        public async Task<Dictionary<int, Info>> SaveFile(IBrowserFile file, Boolean mentee)
        {
            var D = new Dictionary<int, Info>();
            using (var reader = new StreamReader(file.OpenReadStream(1000000)))
            {
                string headerLine = await reader.ReadLineAsync();
                string line;
                int key = 0;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var columns = line.Split(',');
                    string n = columns[0] ?? "";
                    if (!names.Contains(n) && !n.Equals(""))
                    {
                        key += 1;
                        // Use TryParse to avoid throwing exceptions on invalid input
                        int ethnicityWeighting = Int32.TryParse(columns[9], out var tempEthnicityWeighting) ? tempEthnicityWeighting : 0;
                        int identityWeighting = Int32.TryParse(columns[10], out var tempIdentityWeighting) ? tempIdentityWeighting : 0;
                        int academicWeighting = Int32.TryParse(columns[11], out var tempAcademicWeighting) ? tempAcademicWeighting : 0;
                        int graduateWeighting = Int32.TryParse(columns[12], out var tempGraduateWeighting) ? tempGraduateWeighting : 0;
                        int industryWeighting = Int32.TryParse(columns[13], out var tempIndustryWeighting) ? tempIndustryWeighting : 0;
                        int goalWeighting = Int32.TryParse(columns[14], out var tempGoalWeighting) ? tempGoalWeighting : 0;
                        int affinityWeighting = Int32.TryParse(columns[15], out var tempAffinityWeighting) ? tempAffinityWeighting : 0;
                        int numStudent = Int32.TryParse(columns[16], out var tempNumStudent) ? tempNumStudent : 0;

                        var info = new Info
                        {
                            Count = key,
                            Name = columns[0] ?? "",
                            Email = columns[1] ?? "",
                            Major = new List<string>((columns[2] ?? "").Split(";")),
                            Industry = new List<string>((columns[3] ?? "").Split(";")),
                            GradArea = new List<string>((columns[4] ?? "").Split(";")),
                            Goal = new List<string>((columns[5] ?? "").Split(";")),
                            Ethnicity = new List<string>((columns[6] ?? "").Split(";")),
                            Identity = new List<string>((columns[7] ?? "").Split(";")),
                            AffinityGroup = new List<string>((columns[8] ?? "").Split(";")),
                            EthnicityWeighting = ethnicityWeighting,
                            IdentityWeighting = identityWeighting,
                            AcademicWeighting = academicWeighting,
                            GraduateWeighting = graduateWeighting,
                            IndustryWeighting = industryWeighting,
                            GoalWeighting = goalWeighting,
                            AffinityWeighting = affinityWeighting,
                            NumStudent = numStudent
                        };
                        names.Add(n);
                        D.Add(key, info);
                    }
                    else
                    {
                        Console.WriteLine(n + "is already in names");
                        // throw new InvalidDataException("Invalid number of columns in data.");
                    }
                }
            }
            return D;
        }
        
        public async Task<decimal> AcademicSim(Info mentor, Info mentee)
        {
            // Calculate Major Similarity
            if(mentor.Major == null || mentee.Major == null) return 0;
            decimal MajorScore = 0;
            foreach(string mentor_major in mentor.Major) {
                foreach(string mentee_major in mentee.Major) {
                    if(mentor_major.Equals(mentee_major)) {
                        MajorScore += 1;
                    }
                }
            }
            MajorScore = ((mentor.AcademicWeighting + mentee.AcademicWeighting) * MajorScore) / (mentor.Major.Count * mentee.Major.Count);
            return MajorScore;
        }

        public async Task<decimal> IndustrySim(Info mentor, Info mentee)
        {
            // Calculate Industry Similarity
            if(mentor.Industry == null || mentee.Industry == null) return 0;
            decimal IndustryScore = 0;
            foreach(string mentor_ind in mentor.Industry) {
                foreach(string mentee_ind in mentee.Industry) {
                    if(mentor_ind.Equals(mentee_ind)) {
                        IndustryScore += 1;
                    }
                }
            }
            IndustryScore = ((mentor.IndustryWeighting + mentee.IndustryWeighting) * IndustryScore) / (mentor.Industry.Count * mentee.Industry.Count);
            return IndustryScore;
        }

        public async Task<decimal> GraduateSim(Info mentor, Info mentee)
        {
            // Calculate Graduate Interest Similarity
            if(mentor.GradArea == null || mentee.GradArea == null) return 0;
            decimal GraduateScore = 0;
            foreach(string mentor_grad in mentor.GradArea) {
                foreach(string mentee_grad in mentee.GradArea) {
                    if(mentor_grad.Equals("None") || mentee_grad.Equals("None")){
                        return 0;
                    }
                    if(mentor_grad.Equals(mentee_grad)) {
                        GraduateScore += 1;
                    }
                }
            }
            GraduateScore = ((mentor.GraduateWeighting + mentee.GraduateWeighting) * GraduateScore) / (mentor.GradArea.Count * mentee.GradArea.Count);
            return GraduateScore;
        }

        public async Task<decimal> GoalsSim(Info mentor, Info mentee)
        {
            // Calculate Goal Similarity
            if(mentor.Goal == null || mentee.Goal == null) return 0;
            decimal GoalScore = 0;
            foreach(string mentor_goal in mentor.Goal) {
                foreach(string mentee_goal in mentee.Goal) {
                    if(mentor_goal.Equals(mentee_goal)) {
                        GoalScore += 1;
                    }
                }
            }
            GoalScore = ((mentor.GoalWeighting + mentee.GoalWeighting) * GoalScore) / (mentor.Goal.Count * mentee.Goal.Count);
            return GoalScore;
        }

        public async Task<decimal> IdentitySim(Info mentor, Info mentee)
        {
            // Calculate the similarity score for identity
            // Idenitiy includes first gen/LGBTQ+/International/etc

            if(mentor.Identity == null || mentee.Identity == null) return 0;
            decimal IdentityScore = 0;
            foreach(string mentor_identity in mentor.Identity) {
                foreach(string mentee_identity in mentee.Identity) {
                    if(mentor_identity.Equals(mentee_identity)) {
                        IdentityScore += 1;
                    }
                }
            }
            IdentityScore = ((mentor.IdentityWeighting + mentee.IdentityWeighting) * IdentityScore) / (mentor.Identity.Count * mentee.Identity.Count);
            return IdentityScore;
        }

        public async Task<decimal> EthnicitySim(Info mentor, Info mentee)
        {
            // Calculate the similarity score for ethnicity
            if(mentor.Ethnicity == null || mentee.Ethnicity == null) return 0;
            decimal EthnicityScore = 0;
            foreach(string mentor_ethnicity in mentor.Ethnicity) {
                foreach(string mentee_ethnicity in mentee.Ethnicity) {
                    if(mentor_ethnicity.Equals(mentee_ethnicity)) {
                        EthnicityScore += 1;
                    }
                }
            }
            EthnicityScore = ((mentor.EthnicityWeighting + mentee.EthnicityWeighting) * EthnicityScore) / (mentor.Ethnicity.Count * mentee.Ethnicity.Count);
            return EthnicityScore;
        }

        public async Task<decimal> AffinitySim(Info mentor, Info mentee)
        {
            //need to somehow get rid of all of the "other" responses from last year's data cause we plan on geting rid of that answer option
            // Calculate the similarity score for hobbies
            if(mentor.AffinityGroup == null || mentee.AffinityGroup == null) return 0;
            decimal AffinityScore = 0;
            foreach(string mentor_affinity in mentor.AffinityGroup) {
                foreach(string mentee_affinity in mentee.AffinityGroup) {
                    if(mentor_affinity.Equals(mentee_affinity)) {
                        AffinityScore += 1;
                    }
                }
            }
            AffinityScore = ((mentor.AffinityWeighting + mentee.AffinityWeighting) * AffinityScore) / (mentor.AffinityGroup.Count * mentee.AffinityGroup.Count);
            return AffinityScore;
        }

        public async Task<decimal> OverallSim(Info mentor, Info mentee)
        {
            decimal a = await AcademicSim(mentor, mentee);
            decimal b = await IndustrySim(mentor, mentee);
            decimal c = await AffinitySim(mentor, mentee);
            decimal d = await IdentitySim(mentor, mentee);
            decimal e = await EthnicitySim(mentor, mentee);
            decimal f = await GraduateSim(mentor, mentee);
            decimal MarginalScore = a*0.05m + b*0.2m + c*0.02m + d*0.01m + e*0.05m + f*0.1m;
            return MarginalScore/0.33m;
        }

        public async Task<decimal> GetScore(Info mentor, Info mentee)
        {
            // Calculate the scores for each mentor and mentee
            if(mentor == null || mentee == null) return 0;
            try{
                decimal score = 0m;
                score += (await AcademicSim(mentor, mentee) + await IndustrySim(mentor, mentee) + await GraduateSim(mentor, mentee) + await GoalsSim(mentor, mentee) + await IdentitySim(mentor, mentee) + await EthnicitySim(mentor, mentee) + await AffinitySim(mentor, mentee))/(mentor.AcademicWeighting + mentee.AcademicWeighting + mentor.IndustryWeighting + mentee.IndustryWeighting + mentor.GraduateWeighting + mentee.GraduateWeighting + mentor.GoalWeighting + mentee.GoalWeighting + mentor.IdentityWeighting + mentee.IdentityWeighting + mentor.EthnicityWeighting + mentee.EthnicityWeighting + mentor.AffinityWeighting + mentee.AffinityWeighting);
                score += await OverallSim(mentor, mentee);
                return score * 50m;
            }
            catch(Exception ex){
                await LogAsync($"Inside GetScore: {ex.Message}");
            }
            return 0;
        }

        public async Task<decimal[,]> GetScoreMatrix(Dictionary<int,Info> mentorD1, Dictionary<int,Info> menteeD1)
        {
            if (mentorD1 == null || menteeD1 == null)
            {
                throw new InvalidOperationException("mentorD or menteeD is not initialized.");
            }

            // Store the score for each pair
            decimal[,] scores = await DummyMatrix(mentorD1.Count, menteeD1.Count);
            int mentor_id = 1;
            decimal max = 0;
            int ind1 = 0;
            int ind2 = 0;
            foreach (var tor in mentorD1)
            {
                int mentee_id = 1;               
                foreach (var tee in menteeD1)
                {                    
                    Info mentor_info = tor.Value;
                    Info mentee_info = tee.Value;
                    try
                    {
                        scores[mentor_id - 1, mentee_id - 1] = await GetScore(mentor_info, mentee_info);
                        await LogAsync($"Score is {scores[mentor_id - 1, mentee_id - 1]} at {mentor_id - 1} {mentee_id - 1}");
                        if(scores[mentor_id -1, mentee_id -1] > max)
                        {
                            ind1 = mentor_id - 1;
                            ind2 = mentee_id - 1;
                            max = scores[mentor_id - 1, mentee_id - 1];
                        }
                    }
                    catch (Exception ex)
                    {
                        await LogAsync($"{ex.Message}");
                        await LogAsync($"Error at mentor_id: {mentor_id}, mentee_id: {mentee_id}");
                        await LogAsync($"mentorD.Count: {mentorD1.Count}, menteeD.Count: {menteeD1.Count}");
                    }
                    mentee_id++;
                }
                mentor_id++;
            }
            await LogAsync($"Max Score is :{scores[ind1, ind2]} at {ind1}, {ind2}");
            return scores;
        }

        public async Task<decimal[,]> DummyMatrix(int n1, int n2)
        {
            decimal[,] newM = new decimal[n1, n2];
            if(n1 > n2){
                return new decimal[n1, n1];
            }
            if(n2 > n1)
            {
                return new decimal[n2, n2];
            }
            return newM;
        }
        
        public async Task<string[,]> StartMatching(decimal[,] scores, int iter)
        {
            /*
            Matching:
            Match mentors to mentees using the FindAssignments() method by passing into the 'scores' 2D decimal array
            FindAssignments() method uses Hungarian algorithm to return an int[] array called 'matrix' containing the index of the optimal matches
            Example: ideal matches are mentor 1 (index 0) with mentee 7 (index 6), mentor 2 (index 1) with mentee 3 (index 2)...
            The returned 1D array will look like this: [6, 2, ...]
            For each pair, we now know the index of the mentor and the index of the mentee
            We could find their similarity score by searching their index in the scores array: scores[ideal mentor id, ideal mentee id],
            where the ideal mentee id for the specific mentor id is matrix[mentor id]
            We then use this score to determine if they have passed the threshold value or not
            If so, index increments by 1 and we write the pair into a list 'pairingRecords' with variable type 'PairingRecord' from the Models and into the database
            If not, we store the mentor id and mentee id into lists 'mentorReId' and 'menteeReId' in case a rematch will be needed
            After iterating through the matrix, we now have all the pairs that passed the threshold value stored in 'pairingRecords'
            */
            iteration = iter;
            int[] matrix = FindAssignments(scores);
            var pairingRecords = new List<PairingRecord>();
            int index = 0;
            for(int i = 0; i < matrix.Length; i++)
            {
                await LogAsync($"Matched Mentor {i+1} with Mentee {matrix[i]+1} with score {scores[i,matrix[i]]}");
                foreach(var D1 in mentorD){
                    if (D1.Key == i+1)
                    {
                        foreach(var D2 in menteeD)
                        {
                            if (D2.Key == matrix[i] + 1)
                            {
                                double ss = (double)scores[i,matrix[i]];
                                if(ss > thresh)
                                {
                                    var pairingRecord = new PairingRecord() { MentorID = D1.Key, MentorName = D1.Value.Name, MentorEmail = D1.Value.Email, MenteeID = D2.Key, MenteeName = D2.Value.Name, MenteeEmail = D2.Value.Email, SimilarityScore = ss, IterationID = iteration};
                                    index ++;
                                    pairingRecords.Add(pairingRecord);
                                }
                                else
                                {
                                    mentorReId.Add(D1.Key);
                                    menteeReId.Add(D2.Key);
                                }
                            }
                        }
                    }
                }
            }
            await LogAsync($"Index is: {index}");

            /*
            Rematching:
            Initiated if index is lower than the total number of mentees
            Now the lists 'mentorReId' and 'menteeReId' should contain all the mentor id and mentee id for matches that are lower than the threshold value
            We also add all id of the mentors who are willing to take multiple students
            We use the lists of id to get a list of mentor and mentee information:
                'newTor' is a Dictionary<int, Info> that stores the index and Info of the mentor that needs rematching
                'newTee' is a Dictionary<int, Info> that stores the index and Info of the mentee that needs rematching
            We pass the new Dictionaries into the GetScoreMatrix() method to get a new scores matrix called 'newScores'
            And obtain new index matrix 'newMatrix' using the FindAssignments() method
            We then add all into the pairingRecords and into the database
            And we convert the list pairingRecords into a 2D string array 'pairingArray' and return it back to the CalculateScores() method
            



            if(index < menteeNum)
            {
                foreach(var m in mentorD)
                {
                    if(m.Value.NumStudent > 1)
                    {
                        mentorReId.Add(m.Key);
                    }
                }

                if (mentorNum < menteeNum)
                {
                    foreach (var record in menteeD)
                    {
                        bool isPaired = pairingRecords.Any(pr => pr.MenteeID == record.Key);
                        if (!isPaired && !menteeReId.Contains(record.Key))
                        {
                            menteeReId.Add(record.Key);
                        }
                    }
                }
                else if (mentorNum > menteeNum)
                {
                    foreach (var record in mentorD)
                    {
                        bool isPaired = pairingRecords.Any(pr => pr.MentorID == record.Key);
                        if (!isPaired && !mentorReId.Contains(record.Key))
                        {
                            mentorReId.Add(record.Key);
                        }
                    }
                }

                mentorReId = new HashSet<int>(mentorReId).ToList();
                menteeReId = new HashSet<int>(menteeReId).ToList();
                mentorReId.Sort();
                menteeReId.Sort();

                foreach(var x in mentorReId)
                {
                    await LogAsync($"Rematch mentor Id: {x}");
                }
                foreach(var y in menteeReId)
                {
                    await LogAsync($"Rematch mentee Id: {y}");
                }
                var newTor = new Dictionary<int, Info>();
                var newTee = new Dictionary<int, Info>();
                foreach(int x in mentorReId)
                {
                    if(mentorD.ContainsKey(x) && !newTor.ContainsKey(x))
                    {
                        newTor.Add(x,mentorD.GetValueOrDefault(x));
                        await LogAsync($"mentorD contains mentor {x}");
                    }
                }
                foreach(int y in menteeReId)
                {
                    if(menteeD.ContainsKey(y) && !newTee.ContainsKey(y))
                    {
                        newTee.Add(y,menteeD.GetValueOrDefault(y));
                        await LogAsync($"menteeD contains mentee {y}");
                    }
                }

                decimal[,] newScores = await GetScoreMatrix(newTee, newTor);
                int[] newMatrix = FindAssignments(newScores);
                int[] mentorReIdArray = mentorReId.ToArray();
                int[] menteeReIdArray = menteeReId.ToArray();

                for(int x = 0; x < newMatrix.Length; x++)
                {
                    await LogAsync($"mentee: {x}, mentor: {newMatrix[x]}, score : {newScores[x,newMatrix[x]]}");
                }

                for(int y = 0; y < menteeReIdArray.Length; y++){
                    for(int z = 0; z < mentorReIdArray.Length; z++){
                        await LogAsync($"newScores at {y}, {z} is {newScores[y,z]}");
                    }
                }

                for(int x = 0; x < newMatrix.Length; x++)
                {
                    if(newScores[x,newMatrix[x]] > 0)
                    {
                        try{
                            int mentee_index = x;
                            int mentor_index = newMatrix[x];
                            int mentee_id = menteeReIdArray[mentee_index];
                            int mentor_id = mentorReIdArray[mentor_index];
                            await LogAsync($"mentee: {mentee_id}, mentor: {mentor_id}.");
                            Info tor = mentorD.GetValueOrDefault(mentor_id);
                            Info tee = menteeD.GetValueOrDefault(mentee_id);
                            double ss = (double)newScores[x,newMatrix[x]];
                            var pairingRecord = new PairingRecord() { MentorID = mentor_id, MentorName = tor.Name, MentorEmail = tor.Email, MenteeID = mentee_id, MenteeName = tee.Name, MenteeEmail = tee.Email, SimilarityScore = ss, IterationID = iteration};
                            index ++;
                            pairingRecords.Add(pairingRecord);
                        }
                        catch(Exception ex)
                        {
                            await LogAsync($"Error at rematching: {ex.Message} at mentee: {x} and mentor: {newMatrix[x]}");
                        }
                    }
                }
            }
            await LogAsync($"Index is: {index}");
            // Rematching finished
            */


            // Writing all records into the pairingArray to return to CalculateScores() method
            string[,] pairingArray = new string[pairingRecords.Count+1, 9];
            pairingArray[0, 0] = "Id";
            pairingArray[0, 1] = "Mentor ID";
            pairingArray[0, 2] = "Mentor Name";
            pairingArray[0, 3] = "Mentor Email";
            pairingArray[0, 4] = "Mentee ID";
            pairingArray[0, 5] = "Mentee Name";
            pairingArray[0, 6] = "Mentee Email";
            pairingArray[0, 7] = "Similarity Score";
            pairingArray[0, 8] = "Iteration Number"; 
            for (int i = 0; i < pairingRecords.Count; i++)
            {
                var record = pairingRecords[i];
                pairingArray[i + 1, 0] = record.Id.ToString();
                pairingArray[i + 1, 1] = record.MentorID.ToString();
                pairingArray[i + 1, 2] = record.MentorName ?? string.Empty;
                pairingArray[i + 1, 3] = record.MentorEmail ?? string.Empty;
                pairingArray[i + 1, 4] = record.MenteeID.ToString();
                pairingArray[i + 1, 5] = record.MenteeName ?? string.Empty;
                pairingArray[i + 1, 6] = record.MenteeEmail ?? string.Empty;
                pairingArray[i + 1, 7] = record.SimilarityScore.ToString();
                pairingArray[i + 1, 8] = record.IterationID.ToString();
            }
            return pairingArray;
        }

        // Hungarian algorithm starts here
        public static int[] FindAssignments(decimal[,] scores)
        {
            if (scores == null)
            {
                throw new ArgumentNullException(nameof(scores));
            }
            int n = scores.GetLength(0);
            int m = scores.GetLength(1);

            // Converting scores from 2D decimal scores array to 2D double costs array
            double[,] costs = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    costs[i, j] = (double)(100 - scores[i, j]);
                }
            }
            var h = costs.GetLength(0);
            var w = costs.GetLength(1);
            bool rowsGreaterThanCols = h > w;
            
            // If the number of rows is greater than columns, transpose the cost matrix
            if(rowsGreaterThanCols)
            {
                var row = w;
                var col = h;
                var transposeCosts = new double [row, col];
                for(var i = 0; i < row; i++)
                {
                    for (var j = 0; j < col;j++)
                    {
                        transposeCosts[i, j] = costs[j, i];
                    }
                }
                costs = transposeCosts;
                h = row;
                w = col;
            }

            // Find and subtract the smallest value in each row from all elements in that row
            for(var i = 0; i < h; i++)
            {
                var min = double.MaxValue;
                for(var j = 0; j < w; j++)
                {
                    min = Math.Min(min, costs[i, j]);
                }

                for(var j = 0; j < w; j++)
                {
                    costs[i, j] -= min;
                }
            }

            var masks = new byte[h, w]; // Represent the state of the matrix elements: (0) - unmarker, (1) - starred, (2) - primed
            var rowsCovered = new bool[h]; // Boolean array indicating whether each row is covered
            var colsCovered = new bool[w]; // Boolean array indicating whether each column is covered

            // Cover each zero with a star (1) if it does not have any other starred zero in its row or column
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
                    {
                        masks[i, j] = 1;
                        rowsCovered[i] = true;
                        colsCovered[j] = true;
                    }
                }
            }
            ClearCovers(rowsCovered, colsCovered, w, h);
            var path = new Location[w * h]; // Keep track of the sequence of rows and columns that form the alternating path of primed and starred zeros
            var pathStart = default(Location);
            var step = 1;

            // Remaining steps of the Hungarian algorithm begin
            while(step != -1)
            {
                step = step switch
                {
                    1 => RunStep1(masks, colsCovered, w, h),
                    2 => RunStep2(costs, masks, rowsCovered, colsCovered, w, h, ref pathStart),
                    3 => RunStep3(masks, rowsCovered, colsCovered, w, h, path, pathStart),
                    4 => RunStep4(costs, rowsCovered, colsCovered, w, h),
                    _ => step
                };
            }

            // Extract the assignments from the masks array
            var agentsTasks = new int[h]; // Store the final assignments of matching
            // Each index represents an agent (mentor during matching, mentee during rematching), and the value at that index represents the index of the assigned agent (mentee during matching, mentor during rematching).
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(masks[i, j] == 1)
                    {
                        agentsTasks[i] = j;
                        break;
                    }
                    else
                    {
                        agentsTasks[i] = -1;
                    }
                }
            }

            // If the cost matrix was transposed, transpose the assignments back
            if(rowsGreaterThanCols)
            {
                var agentsTasksTranspose = new int[w]; // Temporary storage for assignments when the cost matrix was initially transposed
                for(var i = 0; i < w; i++)
                {
                    agentsTasksTranspose[i] = -1;
                }
            
                for(var j = 0; j < h; j++)
                {
                    agentsTasksTranspose[agentsTasks[j]] = j;
                }
                agentsTasks = agentsTasksTranspose;
            }
            return agentsTasks;
        }

        // Step 1: Cover all columns containing a starred zero and count the number of covered columns
        // If all columns are covered, the solution is optimal. Otherwise, proceed to Step 2
        private static int RunStep1(byte[,] masks, bool[] colsCovered, int w, int h)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(masks[i, j] == 1)
                    {
                        colsCovered[j] = true;
                    }
                }
            }
            var colsCoveredCount = 0;
            for(var j = 0; j < w; j++)
            {
                if(colsCovered[j])
                {
                    colsCoveredCount++;
                }
            }
            if(colsCoveredCount == Math.Min(w, h))
            {
                return -1;
            }
            return 2;
        }

        // Step 2: Find a non-covered zero in the cost matrix and prime it
        // If no non-covered zero found, proceed to step 4
        // If there's a starred zero in the same row, cover the row and uncover the column containing the starred zero. Otherwise, proceed to Step 3.
        private static int RunStep2(double[,] costs, byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, ref Location pathStart)
        {
            if(costs == null)
            {
                throw new ArgumentNullException(nameof(costs));
            }
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            while(true)
            {
                var loc = FindZero(costs, rowsCovered, colsCovered, w, h);
                if(loc.row == -1)
                {
                    return 4;
                }
                masks[loc.row, loc.column] = 2;

                var starCol = FindStarInRow(masks, w, loc.row);
                if(starCol != -1)
                {
                    rowsCovered[loc.row] = true;
                    colsCovered[starCol] = false;
                }
                else
                {
                    pathStart = loc;
                    return 3;
                }
            }
        }

        // Step 3: Construct an alternating path and convert primed and starred zeros
        // Convert primed zeros to starred zeros and starred zeros to unstarred
        private static int RunStep3(byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, Location[] path, Location pathStart)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            var pathIndex = 0;
            path[0] = pathStart;

            while(true)
            {
                var row = FindStarInColumn(masks, h, path[pathIndex].column);
                if(row == -1)
                {
                    break;
                }
                pathIndex++;
                path[pathIndex] = new Location(row, path[pathIndex - 1].column);
                var col = FindPrimeInRow(masks, w, path[pathIndex].row);
                pathIndex++;
                path[pathIndex] = new Location(path[pathIndex - 1].row, col);
            }
            ConvertPath(masks, path, pathIndex + 1);
            ClearCovers(rowsCovered, colsCovered, w, h);
            ClearPrimes(masks, w, h);
            return 1;
        }

        // Step 4: Adjust the cost matrix by the smallest uncovered value
        // Find the smallest uncovered value in the cost matrix
        // Subtract it from all uncovered elements and add (twice since it's both row and column) it to all covered elements
        private static int RunStep4(double[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if(costs == null)
            {
                throw new ArgumentNullException(nameof(costs));
            }
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            var minValue = FindMinimum(costs, rowsCovered, colsCovered, w, h);
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(rowsCovered[i])
                    {
                        costs[i, j] += minValue;
                    }
                    if(!colsCovered[j])
                    {
                        costs[i, j] -= minValue;
                    }
                }
            }
            return 2;
        }

        // Find the smallest value in the uncovered elements of the cost matrix
        private static double FindMinimum(double[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if(costs == null)
            {
                throw new ArgumentNullException(nameof(costs));
            }
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            var minValue = double.MaxValue;
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(!rowsCovered[i] && !colsCovered[j])
                    {
                        minValue = Math.Min(minValue, costs[i, j]);
                    }
                }
            }
            return minValue;
        }

        // Find the column index of a starred zero in the given row
        private static int FindStarInRow(byte[,] masks, int w, int row)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            for(var j = 0; j < w; j++)
            {
                if(masks[row, j] == 1)
                {
                    return j;
                }
            }
            return -1;
        }

        // Find the row index of a starred zero in the given column
        private static int FindStarInColumn(byte[,] masks, int h, int col)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            for(var i = 0; i < h; i++)
            {
                if(masks[i, col] == 1)
                {
                    return i;
                }
            }
            return -1;
        }

        // Find the column index of a primed zero in the given row
        private static int FindPrimeInRow(byte[,] masks, int w, int row)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            for(var j = 0; j < w; j++)
            {
                if(masks[row, j] == 2)
                {
                    return j;
                }
            }
            return -1;
        }

        // Find a non-covered zero in the cost matrix and return its location (row index and column index)
        private static Location FindZero(double[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if(costs == null)
            {
                throw new ArgumentNullException(nameof(costs));
            }
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(costs[i, j] == 0 && !rowsCovered[i] && !colsCovered[j])
                    {
                        return new Location(i, j);
                    }
                }
            }
            return new Location(-1, -1);
        }

        // Convert primed and starred zeros along the path
        private static void ConvertPath(byte[,] masks, Location[] path, int pathLength)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            if(path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            for(var i = 0; i < pathLength; i++)
            {
                masks[path[i].row, path[i].column] = masks[path[i].row, path[i].column] switch
                {
                    1 => 0,
                    2 => 1,
                    _ => masks[path[i].row, path[i].column]
                };
            }
        }

        // Clear all primed zeros
        private static void ClearPrimes(byte[,] masks, int w, int h)
        {
            if(masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }
            for(var i = 0; i < h; i++)
            {
                for(var j = 0; j < w; j++)
                {
                    if(masks[i, j] == 2)
                    {
                        masks[i, j] = 0;
                    }
                }
            }
        }

        // Clear all row and column covers
        private static void ClearCovers(bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if(rowsCovered == null)
            {
                throw new ArgumentNullException(nameof(rowsCovered));
            }
            if(colsCovered == null)
            {
                throw new ArgumentNullException(nameof(colsCovered));
            }
            for(var i = 0; i < h; i++)
            {
                rowsCovered[i] = false;
            }

            for(var j = 0; j < w; j++)
            {
                colsCovered[j] = false;
            }
        }

        private struct Location
        {
            internal readonly int row;
            internal readonly int column;
            internal Location(int row, int col)
            {
                this.row = row;
                this.column = col;
            }
        }
    }
}