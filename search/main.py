from rank import rank

if __name__ == "__main__":
    while True:
        print("Podaj zdanie dla naszego najnowszego cutting-edge AI: ", end="")
        query = input()
        print("Wyciągnięta struktura danych:")
        print(rank(query))