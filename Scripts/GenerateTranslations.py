import pandas as pd
import os

path = (os.path.dirname(os.path.realpath(__file__)) + '\\').replace('\\', '/')

print("Generate Translations.")
file = pd.read_excel(path + "../Game/Content/Resources/Translations/Translations.xlsx")
file.to_csv(path + "../Game/Content/Resources/Translations/Translations.csv", index = None, header=True)
print("End Generate Translations.")