from fastapi import FastAPI
from pydantic import BaseModel
from tagger import generate_tags
from normalizer import normalize_tags

app = FastAPI()


class TagRequest(BaseModel):
    text: str
    existing_tags: list[str] = []
    max_tags: int = 5
    threshold: float = 0.82


class TagResponse(BaseModel):
    matched_tags: list[str]  # full final list (use these)
    new_tags: list[str]  # tags that didn't exist before (create in DB)


@app.post("/extract-tags", response_model=TagResponse)
async def extract_tags(request: TagRequest):
    raw_tags = generate_tags(request.text, request.max_tags)
    result = normalize_tags(raw_tags, request.existing_tags, request.threshold)

    return TagResponse(
        matched_tags=result["matched"],
        new_tags=result["new"]
    )


@app.get("/health")
async def health():
    return {"status": "ok"}