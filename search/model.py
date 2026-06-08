import sys

import pl_core_news_sm

LEMMAS_TO_NUMBERS = {
    # 0 - 9
    "zero": 0,
    "jeden": 1,
    "dwa": 2,
    "trzy": 3,
    "cztery": 4,
    "pięć": 5,
    "sześć": 6,
    "siedem": 7,
    "osiem": 8,
    "dziewięć": 9,

    # 10 - 19
    "dziesięć": 10,
    "jedenaście": 11,
    "dwanaście": 12,
    "trzynaście": 13,
    "czternaście": 14,
    "piętnaście": 15,
    "szesnaście": 16,
    "siedemnaście": 17,
    "osiemnaście": 18,
    "dziewiętnaście": 19,

    # Dziesiątki
    "dwadzieścia": 20,
    "trzydzieści": 30,
    "czterdzieści": 40,
    "pięćdziesiąt": 50,
    "sześćdziesiąt": 60,
    "siedemdziesiąt": 70,
    "osiemdziesiąt": 80,
    "dziewięćdziesiąt": 90,

    # 100
    "sto": 100,
}

# ładowanie lokalnego modelu dla języka polskiego
nlp = pl_core_news_sm.load()

# dodanie reguł naprawczych do źle interpretowanych słów
attribute_ruler = nlp.get_pipe("attribute_ruler")
correct_lemmas = [
    {"patterns": [[{"LOWER": "gb"}]], "attrs": {"LEMMA": "gb"}},
    {"patterns": [[{"LOWER": "mb"}]], "attrs": {"LEMMA": "mb"}},
    {"patterns": [[{"LOWER": "cuda"}]], "attrs": {"LEMMA": "cuda"}},
]
for rule in correct_lemmas:
    attribute_ruler.add(patterns=rule["patterns"], attrs=rule["attrs"])

# definiowanie reguł NLP dla parametrów GPU
entity_ruler = nlp.add_pipe("entity_ruler", before="ner")
entity_ruler.add_patterns([
    # wzorzec dla vram
    {
        "label": "VRAM",
        "pattern": [
            {"TEXT": {"REGEX": r"^\d+(,\d+)?$"}},
            {"LOWER": {"IN": ["mega", "megabajt", "megabajty", "megabajtów", "giga", "gigabajt", "megabajtami",
                              "gigabajty", "gigabajtów", "mb", "gb", "ram", "vram", "megabajta", "gigabajtami",
                              "gigabajta"]}},
        ],
    },
    {
        "label": "VRAM_TEXT",
        "pattern": [
            {"LEMMA": {"IN": list(LEMMAS_TO_NUMBERS.keys())}},
            {"LEMMA": {"IN": list(LEMMAS_TO_NUMBERS.keys())}, "OP": "?"},
            {"LOWER": {"IN": ["mega", "megabajt", "megabajty", "megabajtów", "giga", "gigabajt", "megabajtami",
                              "gigabajty", "gigabajtów", "mb", "gb", "ram", "vram", "megabajta", "gigabajtami",
                              "gigabajta"]}},
        ],
    },

    # wzorzec dla producenta/technologii
    {
        "label": "BRAND",
        "pattern": [
            {"LOWER": {"IN": ["nvidia", "nvidii", "nvidię", "cuda", "cudę", "cudą", "amd", "intel", "intela"]}},
        ],
    },

    # wzorce dla budżetu
    {
        "label": "LOW_BUDGET",
        "pattern": [
            {"LEMMA": {"IN": ["tać", "tani", "taniocha", "najtańszy", "budżetowy", "niedrogi", "ekonomiczny"]}},
        ],
    },
    {
        "label": "LOW_BUDGET",
        "pattern": [
            {"LOWER": {"IN": ["tani", "tania", "tanie", "taniego", "taniej", "taniemu", "tanią", "tanim", "tanich",
                              "tanimi", "tanio", "tańszy", "tańsza", "tańsze", "tańszego", "tańszej", "tańszemu",
                              "tańszą", "tańszym", "tańszych", "tańszymi", "najtańszy", "najtańsza", "najtańsze",
                              "najtańszego", "najtańszej", "najtańszemu", "najtańszą", "najtańszym", "najtańszych",
                              "najtańszymi"]}},
        ],
    },
    {
        "label": "LOW_BUDGET",
        "pattern": [
            {"LEMMA": {"IN": ["niski", "dobry", "okazyjny"]}},
            {"LEMMA": "cena"},
        ],
    }
])


def add_spaces_around_numbers(string: str) -> str:
    new_str = [string[0]]
    for i in range(1, len(string)):
        if string[i-1].isalpha() and string[i].isdigit():
            new_str.append(' ')
        if string[i-1].isdigit() and string[i].isalpha():
            new_str.append(' ')

        if string[i-1].isdigit() and string[i] == '.':
            new_str.append(',')
        else:
            new_str.append(string[i])
    return "".join(new_str)

def extract_gpu_criteria(user_prompt: str) -> dict:
    # preprocessing inputu
    user_prompt = user_prompt.strip()
    user_prompt = add_spaces_around_numbers(user_prompt)
    user_prompt = user_prompt.lower()

    # przepuszczenie inputu użytkownika przez NLP spaCy
    doc = nlp(user_prompt)

    extracted = {
        "min_vram_gb": None,
        "brand": None,
        "is_cheap": False
    }

    # przejrzenie rozpoznanych przez spaCy encji
    for ent in doc.ents:
        if ent.label_ == "VRAM":
            digit = float(ent.text.split()[0].replace(",", "."))
            if digit:
                if digit >= 1024.0:
                    extracted["min_vram_gb"] = digit / 1024.0
                else:
                    extracted["min_vram_gb"] = digit

        elif ent.label_ == "VRAM_TEXT":
            for lemma in ent.lemma_.split():
                try:
                    number = LEMMAS_TO_NUMBERS[lemma.strip()]
                    if not extracted["min_vram_gb"]:
                        extracted["min_vram_gb"] = number
                    else:
                        extracted["min_vram_gb"] += number
                except:
                    print(f"Warning: unknown number lemma: {lemma}.", file=sys.stderr)

        elif ent.label_ == "BRAND":
            val = ent.text.lower()
            if val in ["nvidia", "cuda"]:
                extracted["brand"] = "nvidia"
            elif val == "amd":
                extracted["brand"] = "amd"
            elif val == "intel":
                extracted["brand"] = "intel"

        elif ent.label_ == "LOW_BUDGET":
            extracted["is_cheap"] = True

    return extracted
