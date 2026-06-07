from model import extract_gpu_criteria
from data import gpu_data


def rank(query: str, gpu_data: list[dict]):
    crit = extract_gpu_criteria(query)

    scored = []
    for gpu in gpu_data:
        score = calc_score(gpu, crit)
        scored.append((score, gpu))

    scored.sort(key=lambda x: x[0], reverse=True)

    return [gpu for score, gpu in scored]


def calc_score(gpu, crit) -> float:
    score = 0.0

    if crit["brand"]:
        brand_want = crit["brand"].lower()
        brand_got = gpu["brand"].lower()
        if brand_want == brand_got:
            score += 1.0

    if crit["min_vram_gb"]:
        ram_want = float(crit["min_vram_gb"])
        ram_got = float(gpu["vram_gb"])
        if ram_got >= ram_want:
            score += ram_want / ram_got

    cheap_want = crit["is_cheap"]
    price = gpu["price_per_hour_usd_cents"]
    if cheap_want:
        score += min(0.5, 1.0 / price * 1e2)

    return score

