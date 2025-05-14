using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Info{
        // General Info
        public int Count {get; set;}//
        public string Name {get; set;}//
        public string Email {get; set;}//

        //Experience Info
        public string Year {get; set;}//
        
        
        // Identity Info
        public List<string> Ethnicity {get; set;}//
        public List<string> Identity {get; set;}// Gender
        
        // Academic Info
        public List<string> Major {get; set;}//

        // Graduate School Info
        public List<string> GradArea {get; set;}//
        

        // Industry Info
        public List<string> Industry {get; set;}//

        // Advice Info
        public List<string> Goal {get; set;}// Advice

        // Affinity Group Info
        public List<string> AffinityGroup {get; set;}//

        // Scale Info
        public int EthnicityWeighting {get; set;}//
        public int IdentityWeighting {get; set;}// Gender
        public int AcademicWeighting {get; set;} //
        public int GraduateWeighting {get; set;} //
        public int IndustryWeighting {get; set;}//
        public int GoalWeighting {get; set;} // Advice
        public int AffinityWeighting {get; set;}//

        // Willingness to take multiple mentees
        public int NumStudent {get; set;}//
    }
}