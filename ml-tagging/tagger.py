import spacy
from keybert import KeyBERT
from sentence_transformers import SentenceTransformer
import re

nlp = spacy.load("en_core_web_sm")
shared_model = SentenceTransformer("all-MiniLM-L6-v2")
kw_model = KeyBERT(model=shared_model)

def extract_spacy_tags(text: str) -> list[str]:
    doc = nlp(text)
    tags = []

    for token in doc:
        if token.pos_ in ("PROPN", "NOUN") and not token.is_stop and len(token.text) > 1:
            tags.append(token.text.lower())

    for chunk in doc.noun_chunks:
        clean = chunk.text.lower().strip()
        words = clean.split()
        if len(words) > 1 and not any(w in tags for w in words):
            tags.append(clean)

    return list(dict.fromkeys(tags))

def extract_keybert_tags(text: str, max_tags: int) -> list[str]:
    keywords = kw_model.extract_keywords(
        text,
        keyphrase_ngram_range=(1, 1),
        stop_words='english',
        top_n=max_tags,
        use_mmr=True,
        diversity=0.7
    )
    return [kw[0].lower().strip() for kw in keywords]

def generate_tags(text: str, max_tags: int = 5) -> list[str]:
    spacy_tags = extract_spacy_tags(text)
    keybert_tags = extract_keybert_tags(text, max_tags)

    # Combine — spaCy first since it's more precise,
    # then append KeyBERT tags that aren't already covered
    combined = spacy_tags.copy()
    for tag in keybert_tags:
        if tag not in combined:
            combined.append(tag)

    print(f"spaCy tags:   {spacy_tags}")
    print(f"KeyBERT tags: {keybert_tags}")
    print(f"Combined:     {combined[:max_tags]}")

    return combined[:max_tags]