import subprocess
import os

path = (os.path.dirname(os.path.realpath(__file__)) + '\\').replace('\\', '/')

print("Setup.")
subprocess.call("python {0}/GenerateTranslations.py".format(path), shell=True)
subprocess.call("python {0}/Build.py".format(path), shell=True)
print("End Setup.")