#pip install torch
#pip install transformers
#pip install sklearn
#pip install pandas
import torch
from transformers import AutoTokenizer, AutoModel
from sklearn.metrics.pairwise import cosine_similarity
import numpy as np
import pandas as pd

model_name = 'sentence-transformers/all-MiniLM-L6-v2'
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModel.from_pretrained(model_name)


#pre-process: tokenize and encode

#Tokenize:
#process data by tokenizing text
# AutoTokenizer uses a tokenizer from BERT (Bidirectional Encoder Representations from Transformers) implementing WordPiece tokenization
#given a phrase, split by spaces, punctuation, and also subwords (prefixes, suffixes, etc)

#Encode
#pass tokenized text through a model with no gradients to get embeddings (high-dimensional vectors)
#models are already trained to map semantically similar phrases to be close to each other in the embedding space (similar vectors)

def preprocess(data):
    inputs = tokenizer(data, return_tensors='pt', padding=True, truncation=True)
    with torch.no_grad():
        outputs = model(**inputs)
    embeddings = outputs.last_hidden_state.mean(dim=1)
    return embeddings.numpy()


#normalize scores between 0 and 1
def normalize(simMatrix):
    maxVal = np.max(simMatrix)
    minVal = np.min(simMatrix)
    normSimMatrix = (simMatrix - minVal) / (maxVal - minVal)
    return normSimMatrix


#given the embeddings vectors, similarity is calculated by finding the cosine angle between two vectors (cosine similarity)
def calcSimMatrix(data):
    embeddings = preprocess(data)
    simMatrix = cosine_similarity(embeddings)
    norm = normalize(simMatrix)
    normSimMatrix = pd.DataFrame(norm)
    return normSimMatrix

def main():

    #data

    majors = ["African American Studies", "Art History", "Asian and Middle Eastern Studies", "Biology",
              "Biomedical Engineering", "Biophysics", "Brazilian and Global Portuguese Studies",
              "Chemistry", "Civil Engineering", "Classical Civilization", "Classical Languages",
              "Computer Science", "Cultural Anthropology", "Dance", "Earth and Ocean Sciences",
              "Economics", "Electrical and Computer Engineering", "English", "Environmental Engineering",
              "Environmental Sciences", "Environmental Sciences and Policy", "Evolutionary Anthropology",
              "French Studies", "Gender Sexuality and Feminist Studies", "German", "Global Cultural Studies",
              "Global Health", "History", "International Comparative Studies",
              "Italian Studies", "Linguistics", "Mathematics", "Mechanical Engineering",
              "Medieval and Renaissance Studies", "Music", "Neuroscience", "Philosophy", "Physics",
              "Political Science", "Psychology", "Public Policy Studies", "Religious Studies",
              "Romance Studies", "Russian", "Slavic and Eurasian Studies", "Sociology",
              "Spanish Latin American and Latino/a Studies", "Statistical Science", "Theater Studies",
              "Visual Arts", "Visual and Media Studies", "Undecided"]

    minors = ["African & African American Studies", "Art History", "Asian American & Diaspora Studies",
        "Asian and Middle Eastern Studies", "Biology", "Brazilian and Global Portuguese Studies",
        "Chemistry", "Cinematic Arts", "Classical Archaeology", "Classical Civilization",
        "Computational Biology and Bioinformatics", "Computational Media", "Computer Science",
        "Creative Writing", "Cultural Anthropology", "Dance", "Earth and Climate Sciences",
        "Economics", "Education", "Electrical and Computer Engineering", "Energy Engineering*",
        "English", "Environmental Sciences and Policy", "Evolutionary Anthropology", "Finance",
        "French Studies", "Gender, Sexuality, and Feminist Studies", "German", "Global Cultural Studies",
        "Global Health", "Greek", "History", "Inequality Studies", "Italian Studies", "Latin",
        "Linguistics", "Machine Learning & Artificial Intelligence", "Marine Science & Conservation",
        "Mathematics", "Medical Sociology", "Medieval and Renaissance Studies", "Music",
        "Musical Theater", "Neuroscience", "Philosophy", "Photography", "Physics",
        "Polish Culture and Language", "Political Science", "Psychology", "Religious Studies",
        "Russian and East European Literatures in Translation", "Russian Culture and Language",
        "Sexuality Studies", "Sociology", "Spanish Studies", "Statistical Science", "Theater Studies",
        "Visual Arts", "Visual and Media Studies"]


    industries = ["Technology / Software / IT", "Finance / Investment / Banking", "Medical",
                  "Consulting and professional services", "Healthcare", "Nursing", "Education",
                  "Journalism", "Marketing / Advertising", "Arts / Creative Industries",
                  "Non-Profit and Social Services", "Law / Legal Services", "Hospitality and Tourism",
                  "Manufacturing and engineering", "Retail and e-commerce", "Energy and Utilities",
                  "Politics / Government and public sector", "Real estate", "Construction", "Food and beverage",
                  "Media, Entertainment, and Sports", "Transportation and logistics", "Fashion and Apparel",
                  "STEM (Science, Tech, Engineering, Math)", "Artificial Intelligence / Machine Learning / Data Science",
                  "Pharmaceutical", "Biotechnology", "Research and Development", "Telecommunications",
                  "Aerospace and Defense", "Agricultural and Food Science", "Environmental Science",
                  "Product / Product Management / UX Design", "Veterinary Medicine", "Venture Capital",
                  "Accounting", "Private Equity", "Architecture and Design", "Military", "International Relations",
                  "Academia / Higher Ed", "Religion", "Physical Therapy", "Dentistry"]


    majorSimMatrix = calcSimMatrix(majors)
    majorSimMatrix.to_csv("majorsSimMatrix.csv")
    print("Major Similarity Matrix:")
    print(majorSimMatrix)

    minorSimMatrix = calcSimMatrix(minors)
    minorSimMatrix.to_csv("minorsSimMatrix.csv");
    print("Minor Similarity Matrix:")
    print(minorSimMatrix)

    industriesSimMatrix = calcSimMatrix(industries);
    industriesSimMatrix.to_csv("industriesSimMatrix.csv")
    print("Industry Similarity Matrix:")
    print(industriesSimMatrix)







if __name__ == "__main__":
    main()
