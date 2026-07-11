"""
Erzeugt den XOR+Base64-obfuskierten String fuer SecretVault.cs.

Aufruf:
    python obfuscate_key.py sk-dein-neuer-key

Ausgabe einfach in Services/SecretVault.cs bei DeepSeekApiKeyEncoded einsetzen.
Passphrase muss identisch mit der in SecretVault.cs bleiben.
"""
import base64
import sys

PASSPHRASE = "WhimsyTales_v2_Salt#2026"


def xor_encode(plain: str, passphrase: str) -> str:
    pb = passphrase.encode("utf-8")
    data = plain.encode("utf-8")
    out = bytes(b ^ pb[i % len(pb)] for i, b in enumerate(data))
    return base64.b64encode(out).decode("ascii")


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python obfuscate_key.py <neuer-api-key>")
        sys.exit(1)

    print(xor_encode(sys.argv[1], PASSPHRASE))
