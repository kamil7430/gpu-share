import model

EXPECTED_RESULTS = [
    # 1-10: Standardowe zapytania (NVIDIA)
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True},  # 1
    {"min_vram_gb": 12, "brand": "nvidia", "is_cheap": True},  # 2
    {"min_vram_gb": 16, "brand": "nvidia", "is_cheap": True},  # 3
    {"min_vram_gb": 8, "brand": "nvidia", "is_cheap": True},  # 4
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": False},  # 5
    {"min_vram_gb": 12, "brand": "nvidia", "is_cheap": False},  # 6
    {"min_vram_gb": 16, "brand": "nvidia", "is_cheap": False},  # 7
    {"min_vram_gb": 48, "brand": "nvidia", "is_cheap": False},  # 8
    {"min_vram_gb": 80, "brand": "nvidia", "is_cheap": False},  # 9
    {"min_vram_gb": 4, "brand": "nvidia", "is_cheap": True},  # 10

    # 11-20: Standardowe zapytania (AMD / Intel)
    {"min_vram_gb": 16, "brand": "amd", "is_cheap": True},  # 11
    {"min_vram_gb": 24, "brand": "amd", "is_cheap": True},  # 12
    {"min_vram_gb": 8, "brand": "amd", "is_cheap": True},  # 13
    {"min_vram_gb": 16, "brand": "intel", "is_cheap": True},  # 14
    {"min_vram_gb": 8, "brand": "intel", "is_cheap": True},  # 15
    {"min_vram_gb": 20, "brand": "amd", "is_cheap": False},  # 16
    {"min_vram_gb": 24, "brand": "amd", "is_cheap": False},  # 17
    {"min_vram_gb": 16, "brand": "amd", "is_cheap": False},  # 18
    {"min_vram_gb": 12, "brand": "amd", "is_cheap": True},  # 19
    {"min_vram_gb": 16, "brand": "intel", "is_cheap": False},  # 20

    # 21-30: Bez podawania marki (Sam VRAM i budżet)
    {"min_vram_gb": 24, "brand": None, "is_cheap": True},  # 21
    {"min_vram_gb": 16, "brand": None, "is_cheap": True},  # 22
    {"min_vram_gb": 12, "brand": None, "is_cheap": True},  # 23
    {"min_vram_gb": 8, "brand": None, "is_cheap": True},  # 24
    {"min_vram_gb": 24, "brand": None, "is_cheap": False},  # 25
    {"min_vram_gb": 16, "brand": None, "is_cheap": False},  # 26
    {"min_vram_gb": 48, "brand": None, "is_cheap": False},  # 27
    {"min_vram_gb": 4, "brand": None, "is_cheap": True},  # 28
    {"min_vram_gb": 6, "brand": None, "is_cheap": True},  # 29
    {"min_vram_gb": 32, "brand": None, "is_cheap": False},  # 30

    # 31-40: Sam budżet / sama marka (Brak sprecyzowanego VRAM)
    {"min_vram_gb": None, "brand": "nvidia", "is_cheap": True},  # 31
    {"min_vram_gb": None, "brand": "amd", "is_cheap": True},  # 32
    {"min_vram_gb": None, "brand": "intel", "is_cheap": True},  # 33
    {"min_vram_gb": None, "brand": "nvidia", "is_cheap": False},  # 34
    {"min_vram_gb": None, "brand": "amd", "is_cheap": False},  # 35
    {"min_vram_gb": None, "brand": None, "is_cheap": True},  # 36
    {"min_vram_gb": None, "brand": "nvidia", "is_cheap": True},  # 37
    {"min_vram_gb": None, "brand": "amd", "is_cheap": True},  # 38
    {"min_vram_gb": None, "brand": None, "is_cheap": True},  # 39
    {"min_vram_gb": None, "brand": "intel", "is_cheap": True},  # 40

    # 41-50: Trudna gramatyka, odmiany i synonimy ceny
    {"min_vram_gb": 16, "brand": "nvidia", "is_cheap": True},  # 41
    {"min_vram_gb": 12, "brand": "nvidia", "is_cheap": True},  # 42
    {"min_vram_gb": 24, "brand": None, "is_cheap": True},  # 43
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True},  # 44
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True},  # 45
    {"min_vram_gb": 16, "brand": "amd", "is_cheap": True},  # 46
    {"min_vram_gb": 8, "brand": "nvidia", "is_cheap": True},  # 47
    {"min_vram_gb": 12, "brand": "amd", "is_cheap": True},  # 48
    {"min_vram_gb": 16, "brand": "intel", "is_cheap": True},  # 49
    {"min_vram_gb": 24, "brand": "amd", "is_cheap": True},  # 50

    # 51-60: Zapis słowny liczb i literówki
    {"min_vram_gb": 24, "brand": None, "is_cheap": True},  # 51
    {"min_vram_gb": 16, "brand": "amd", "is_cheap": True},  # 52
    {"min_vram_gb": 12, "brand": "nvidia", "is_cheap": True},  # 53
    {"min_vram_gb": 8, "brand": "nvidia", "is_cheap": True},  # 54
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True},  # 55
    {"min_vram_gb": 16, "brand": None, "is_cheap": True},  # 56
    {"min_vram_gb": 4, "brand": "nvidia", "is_cheap": True},  # 57
    {"min_vram_gb": 48, "brand": None, "is_cheap": False},  # 58
    {"min_vram_gb": 12, "brand": "amd", "is_cheap": True},  # 59
    {"min_vram_gb": 8, "brand": "intel", "is_cheap": True},  # 60

    # 61-70: Jednostki w megabajtach (MB) i specyficzny zapis
    {"min_vram_gb": 8, "brand": "nvidia", "is_cheap": True},  # 61
    {"min_vram_gb": 16, "brand": "amd", "is_cheap": True},  # 62
    {"min_vram_gb": 4, "brand": "nvidia", "is_cheap": True},  # 63
    {"min_vram_gb": 12, "brand": "nvidia", "is_cheap": True},  # 64
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True},  # 65
    {"min_vram_gb": 16, "brand": "amd", "is_cheap": True},  # 66
    {"min_vram_gb": 8, "brand": "intel", "is_cheap": True},  # 67
    {"min_vram_gb": 32, "brand": "nvidia", "is_cheap": False},  # 68
    {"min_vram_gb": 49, "brand": "nvidia", "is_cheap": False}, # 69
    {"min_vram_gb": 2, "brand": "nvidia", "is_cheap": True},  # 70

    # 71-80: Zapytania z szumem informacyjnym (Długie prompty)
    {"min_vram_gb": 12, "brand": "nvidia", "is_cheap": True},  # 71
    {"min_vram_gb": 16, "brand": "amd", "is_cheap": True},  # 72
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True},  # 73
    {"min_vram_gb": 8, "brand": "nvidia", "is_cheap": True},  # 74
    {"min_vram_gb": 12, "brand": "nvidia", "is_cheap": True},  # 75
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True},  # 76
    {"min_vram_gb": 16, "brand": "nvidia", "is_cheap": True},  # 77
    {"min_vram_gb": 48, "brand": "nvidia", "is_cheap": False},  # 78
    {"min_vram_gb": 24, "brand": "amd", "is_cheap": True},  # 79
    {"min_vram_gb": 8, "brand": "intel", "is_cheap": True},  # 80

    # 81-90: Zapytania techniczne (Słowa kluczowe)
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True},  # 81
    {"min_vram_gb": 16, "brand": "amd", "is_cheap": True},  # 82
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": False},  # 83
    {"min_vram_gb": 12, "brand": "nvidia", "is_cheap": True},  # 84
    {"min_vram_gb": 16, "brand": "nvidia", "is_cheap": True},  # 85
    {"min_vram_gb": 8, "brand": "amd", "is_cheap": True},  # 86
    {"min_vram_gb": 48, "brand": "nvidia", "is_cheap": False},  # 87
    {"min_vram_gb": 24, "brand": "amd", "is_cheap": True},  # 88
    {"min_vram_gb": 16, "brand": "intel", "is_cheap": True},  # 89
    {"min_vram_gb": 80, "brand": "nvidia", "is_cheap": False},  # 90

    # 91-100: Przypadki brzegowe, negacje, "cena nie gra roli"
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True},  # 91
    {"min_vram_gb": 16, "brand": "amd", "is_cheap": True},  # 92
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True},  # 93
    {"min_vram_gb": 8, "brand": "nvidia", "is_cheap": False},  # 94
    {"min_vram_gb": 24, "brand": "amd", "is_cheap": False},  # 95
    {"min_vram_gb": 4, "brand": None, "is_cheap": True},  # 96
    {"min_vram_gb": 12, "brand": "nvidia", "is_cheap": False},  # 97
    {"min_vram_gb": 16, "brand": "amd", "is_cheap": False},  # 98
    {"min_vram_gb": 48, "brand": "nvidia", "is_cheap": False},  # 99
    {"min_vram_gb": 24, "brand": "nvidia", "is_cheap": True}  # 100
]

TEST_SENTENCES = [
    # 1-10: Standardowe zapytania (NVIDIA)
    "Tanie GPU do trenowania modeli językowych, najlepiej z dużą ilością 24 GB vram i dobrym wsparciem dla cuda.",
    "Szukam taniej karty od nvidia z minimum 12gb pamięci.",
    "Niedroga karta graficzna nvidia posiadająca 16 gb vram.",
    "Jaka jest najtańsza karta od nvidia mająca przynajmniej 8gb vram?",
    "Potrzebuję mocnej karty nvidia z 24gb vram, cena nie ma znaczenia.",
    "Szukam wydajnego procesora graficznego nvidia z 12 gb pamięci.",
    "Interesuje mnie profesjonalne rozwiązanie nvidia rtx z 16gb vram.",
    "Nvidia z 48gb vram do uruchamiania wielkich modeli lokalnie.",
    "Potrzebuję potężnej karty nvidia a100 z 80gb pamięci dla instytutu.",
    "Najtańsze możliwe gpu nvidia z 4gb vram do prostych testów.",

    # 11-20: Standardowe zapytania (AMD / Intel)
    "Potrzebuję budżetowe gpu do stabilnej dyfuzji, najlepiej amd 16 gb vram.",
    "Szukam czegoś od amd z 24gb pamięci w niskiej cenie.",
    "Tania karta amd radosn posiadająca 8gb vram.",
    "Ekonomiczna karta graficzna intel arc z pamięcią 16gb.",
    "Szukam budżetowej karty intel posiadającej 8 gb vram.",
    "Potrzebuję wydajnego gpu amd rx z pamięcią 20gb do pracy.",
    "Najnowszy potwór od amd z 24gb vram do głębokiego uczenia.",
    "Karta graficzna amd radeon posiadająca 16gb vram dla programisty.",
    "Niedrogie gpu od amd posiadające minimum 12 gb pamięci.",
    "Intel arc z 16gb vram, zależy mi na wydajności, cena bez znaczenia.",

    # 21-30: Bez podawania marki (Sam VRAM i budżet)
    "Chcę kupić tanie gpu z pamięcią 24gb do uczenia maszynowego.",
    "Szukam budżetowej karty graficznej mającej 16 gb vram.",
    "Jakaś tania opcja z 12gb vram do zabawy z sieciami neuronowymi?",
    "Potrzebuję najtańszej karty na rynku, która ma chociaż 8gb pamięci.",
    "Szukam bardzo mocnego gpu z 24gb vram do renderingu i ai.",
    "Interesuje mnie wydajna karta graficzna wyposażona w 16gb vram.",
    "Potrzebuję ogromnej ilości pamięci, minimum 48gb vram w karcie.",
    "Ekonomiczne, proste gpu posiadające zaledwie 4gb vram.",
    "Tania karta graficzna z nietypową ilością 6gb vram.",
    "Potrzebuję zaawansowanego układu graficznego posiadającego 32gb vram.",

    # 31-40: Sam budżet / sama marka (Brak sprecyzowanego VRAM)
    "Interesuje mnie tania karta nvidia do głębokiego uczenia.",
    "Szukam czegoś budżetowego ze stajni amd do programowania sieci.",
    "Jakaś ekonomiczna i tania karta graficzna od intela?",
    "Potrzebuję topowej karty nvidia o maksymalnej wydajności.",
    "Szukam najnowszego, flagowego układu graficznego od amd.",
    "Niedrogie i energooszczędne gpu do serwera domowego.",
    "Szukam budżetowej karty graficznej, producent musi być nvidia.",
    "Chcę kupić tanią kartę graficzną, najlepiej marki amd.",
    "Jakie ekonomiczne gpu polecacie do nauki podstaw sztucznej inteligencji?",
    "Szukam taniej karty z nowej serii intel arc.",

    # 41-50: Trudna gramatyka, odmiany i synonimy ceny
    "Znajdź mi coś w ekonomicznej cenie ze stajni nvidia, co ma 16gb na pokładzie.",
    "Interesują mnie wyłącznie tanich kart od nvidia, najlepiej z pamięcią minimum 12 gb.",
    "Potrzebuję karty z dużą ilością vramu, najlepiej 24 gigabajtów, ale w niska cena.",
    "Jakaś ekonomiczna opcja z technologią cuda i dużym vram (24gb)?",
    "CUDA jest dla mnie ważna, vram minimum 24gb, ale żeby cena za godzinę była niska i tania.",
    "Poszukuję budżetowej architektury z szesnastoma gigabajtami pamięci od amd.",
    "8 gb vram to absolutne minimum, marka nvidia, szukam czegoś w budżetowej półce.",
    "Niedrogimi kartami od amd z 12gb vramu jestem zainteresowany.",
    "Taniemu układowi intel arc z 16gb vram chętnie się przyjrzę.",
    "Szukam czegoś niedrogiego od amd, warunek to 24 gigabajty pamięci.",

    # 51-60: Zapis słowny liczb i literówki
    "Chciałbym kupić niedrogie gpu, które ma przynajmniej dwadzieścia cztery gigabajty pamięci.",
    "Potrzebuję budżetowej karty amd z szesnastoma gigabajtami pamięci.",
    "Szukam taniej nvidia rtx posiadającej dwanaście gb vram.",
    "Ekonomiczna karta nvidia mająca osiem gigabajtów pamięci.",
    "Niska cena, marka nvidia oraz dwadzieścia cztery gb vram na dane.",
    "Szukam niedrogiego rozwiązania posiadającego szesnaście gigabajtów vram.",
    "Tania karta od nvidia wyposażona w cztery gigabajty pamięci.",
    "Potrzebuję czterdzieści osiem gigabajtów pamięci na ogromny model językowy.",
    "Niedroga architektura amd z dwunastoma gigabajtami vram.",
    "Tani intel arc wyposażony w osiem gigabajtów pamięci operacyjnej.",

    # 61-70: Jednostki w megabajtach (MB) i specyficzny zapis
    "Tanie gpu nvidia mające na pokładzie dokładnie 8192 mb vram.",
    "Szukam niedrogiej karty amd posiadającej 16384 mb pamięci.",
    "Budżetowa nvidia wyposażona w 4096mb pamięci dla projektów.",
    "Potrzebuję taniej karty nvidia z minimum 12000 mb pamięci vram.",
    "Ekonomiczne gpu od nvidia posiadające 24576 mb vram.",
    "Tania karta od amd z pamięcią rzędu 16384mb vram.",
    "Budżetowy intel arc posiadający dokładnie 8192mb vram.",
    "Potrzebuję 32768 mb vram w karcie nvidia do zaawansowanych obliczeń.",
    "Nvidia rtx z pamięcią wynoszącą 49152 mb vram do renderowania modeli.",
    "Tania, stara karta graficzna nvidia mająca zaledwie 2048 mb pamięci.",

    # 71-80: Zapytania z szumem informacyjnym (Długie prompty)
    "Cześć, jestem studentem i robię projekt na zajęcia z AI, potrzebuję tanie gpu z 12gb vram od nvidia, pomożesz?",
    "Do gier i pracy przy sieciach neuronowych szukam niedrogiej karty, najlepiej 16gb vram, może być amd.",
    "Moje fundusze są ograniczone, więc interesuje mnie wyłącznie niska cena i 24gb vram z obsługą cuda.",
    "Profesor kazał nam znaleźć jakieś tanie rozwiązanie z 8 gigabajtami pamięci od nvidia, co macie?",
    "Ekologiczne i przede wszystkim ekonomiczne gpu, nvidia z pamięcią 12 gb do micro-serwera.",
    "Chcę postawić lokalnego bota, więc szukam taniego rozwiązania z 24gb vram, najlepiej nvidia.",
    "Witam, poszukuję budżetowej karty do laboratorium studenckiego, nvidia 16gb vram będzie idealna.",
    "Robię zaawansowane badania naukowe i potrzebuję potężnej karty 48gb vram od nvidia, budżet nie gra roli.",
    "Dla startupu szukamy oszczędności, interesuje nas wyłącznie tania karta amd mająca 24gb vram.",
    "Uczę się programować w oneapi i szukam taniego intela z 8gb pamięci do nauki kompilacji.",

    # 81-90: Zapytania techniczne (Słowa kluczowe)
    "Potrzebuję dobrego wsparcia dla cuda oraz 24gb vram, szukam budżetowej karty graficznej.",
    "Szukam karty amd z obsługą rocm i 16gb vram w bardzo okazyjnej cenie.",
    "Do trenowania modeli llm potrzebuję potężnego akceleratora nvidia z 24gb vram.",
    "Stable diffusion wymaga sporego vramu, znajdź mi tanią nvidia 12gb.",
    "Do uruchomienia llama 3 potrzebuję taniej karty z technologią cuda oraz 16gb pamięci.",
    "Chcę uruchomić rocm na linuxie, jaka jest najtańsza karta amd z 8gb vram?",
    "Do fine-tuningu modeli w chmurze potrzebuję maszynę nvidia z 48gb vram.",
    "Wydajne przetwarzanie tensorów, wsparcie dla pytorch, karta amd 24gb w niskiej cenie.",
    "Intel arc ze wsparciem dla xess i 16gb pamięci do eksperymentów z ai w dobrej cenie.",
    "Potrzebuję bezkompromisowej wydajności fp32, nvidia h100 z 80gb vram dla korporacji.",

    # 91-100: Przypadki brzegowe, negacje, "cena nie gra roli"
    "Nvidia cuda 24 gb vram tanio.",
    "Tanie amd 16gb.",
    "Potrzebuję gpu nvidia rtx 3090, niska cena, pamięć 24gb.",
    "Szukam drogiej karty nvidia, która ma mało vramu, np. 8 gb.",
    "Potrzebuję super wydajnego potwora od amd z 24gb vram, cena nie gra roli.",
    "Jakaś tania karta z 4gb vram dla mało wymagającego modelu.",
    "Interesuje mnie droga, luksusowa karta nvidia rtx z 12gb pamięci.",
    "Szukam profesjonalnego, drogiego akceleratora amd radosn z 16gb vram.",
    "Najwyższa półka cenowa, topowa wydajność, nvidia rtx z 48gb vram.",
    "Absolutna taniocha od nvidia, byleby miało te 24gb pamięci na dane."
]


def run_evaluation_tests():
    total_tests = len(TEST_SENTENCES)
    passed_tests = 0

    print("=" * 70)
    print("                URUCHAMIANIE TESTÓW EWALUACYJNYCH                  ")
    print("=" * 70)

    for idx, (sentence, expected) in enumerate(zip(TEST_SENTENCES, EXPECTED_RESULTS), 1):
        actual = model.extract_gpu_criteria(sentence)

        is_correct = actual == expected

        if is_correct:
            passed_tests += 1
            status = "✅ OK"
        else:
            status = "❌ BŁĄD"

        print(f"[{idx:02d}] {status} | Prompt: \"{sentence[:50]}...\"")
        if not is_correct:
            print(f"     -> Oczekiwano: {expected}")
            print(f"     -> Otrzymano : {actual}")

    print("-" * 70)
    accuracy = (passed_tests / total_tests) * 100
    print(f"PODSUMOWANIE:")
    print(f"Poprawnie zinterpretowane: {passed_tests} z {total_tests}")
    print(f"Skuteczność modelu (Accuracy): {accuracy:.2f}%")
    print("=" * 70)


if __name__ == "__main__":
    run_evaluation_tests()