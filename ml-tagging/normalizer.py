from sentence_transformers import SentenceTransformer, util

model = SentenceTransformer("all-MiniLM-L6-v2")

def normalize_tags(
        new_tags: list[str],
        existing_tags: list[str],
        threshold: float = 0.82
) -> dict:
    """
    For each generated tag, check if a semantically similar tag already exists.
    Returns matched tags (reuse existing) and new tags (create in DB).
    """
    if not existing_tags:
        return {"matched": new_tags, "new": new_tags}

    existing_embeddings = model.encode(existing_tags, convert_to_tensor=True)

    matched = []
    new = []

    for tag in new_tags:
        tag_embedding = model.encode(tag, convert_to_tensor=True)
        similarities = util.cos_sim(tag_embedding, existing_embeddings)[0]

        best_score = similarities.max().item()
        best_index = similarities.argmax().item()

        if best_score >= threshold:
            matched.append(existing_tags[best_index])  # reuse existing
        else:
            matched.append(tag)  # new tag
            new.append(tag)

    return {"matched": matched, "new": new}