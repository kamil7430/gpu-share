from model import extract_gpu_criteria
from data import get_devices


def rank(query: str):
    crit = extract_gpu_criteria(query)
    print(f"ranking with query: {query}\ncriteria: {crit}")

    scored = []
    for gpu in get_devices():
        score = calc_score(gpu, crit)
        scored.append((score, gpu))

    scored.sort(key=lambda x: x[0], reverse=True)
    print("finished: {scored}")

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
        ram_got = float(gpu["vram_mb"]) / 2**10
        if ram_got >= ram_want:
            score += ram_want / ram_got

    cheap_want = crit["is_cheap"]
    price = gpu["price_per_hour_usd_cents"]
    if cheap_want:
        score += min(0.5, 1.0 / price * 1e2)

    return score

