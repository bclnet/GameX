from TwoFish import TwoFish_decrypt
from Crypto.Util.Padding import pad, unpad
from Crypto.Util.strxor import strxor

class TwofishCBC:
    """
    A PyCryptodome-compatible wrapper for the Twofish cipher in CBC mode.
    """
    block_size = 16

    def __init__(self, key, iv):
        if len(key) not in [16, 24, 32]:
            raise ValueError("Key must be 16, 24, or 32 bytes long.")
        if len(iv) != self.block_size:
            raise ValueError(f"IV must be {self.block_size} bytes long.")
        
        self.key = key.hex()
        self.iv = iv
        self._cipher = CoreTwofish(key)

    # def encrypt(self, plaintext):
    #     """Encrypts data using Cipher Block Chaining (CBC)."""
    #     # Ensure the data is padded to the 16-byte block size boundary
    #     padded_data = pad(plaintext, self.block_size)
    #     ciphertext = bytearray()
    #     previous_block = self.iv

    #     # Process data block by block (16 bytes each)
    #     for i in range(0, len(padded_data), self.block_size):
    #         current_block = padded_data[i:i + self.block_size]
    #         # XOR step for CBC mode
    #         xored_block = strxor(current_block, previous_block)
    #         # Core block encryption
    #         encrypted_block = self._cipher.encrypt(xored_block)
    #         ciphertext.extend(encrypted_block)
    #         previous_block = encrypted_block

    #     return bytes(ciphertext)

    def decrypt(self, ciphertext):
        """Decrypts data using Cipher Block Chaining (CBC)."""
        if len(ciphertext) % self.block_size != 0:
            raise ValueError("Ciphertext length must be a multiple of the block size.")

        plaintext = bytearray()
        previous_block = self.iv

        # Process data block by block
        for i in range(0, len(ciphertext), self.block_size):
            encrypted_block = ciphertext[i:i + self.block_size]
            # Core block decryption
            decrypted_block = TwoFish_decrypt(encrypted_block, self.key, 'CBC')
            # Inverse XOR step for CBC mode
            original_block = strxor(decrypted_block, previous_block)
            plaintext.extend(original_block)
            previous_block = encrypted_block

        # Remove PKCS#7 padding before returning the true message
        return unpad(bytes(plaintext), self.block_size)

# Factory function matching PyCryptodome's API pattern
def new(key, iv):
    """
    Create a new Twofish cipher object.
    
    :param key: The symmetric key (16, 24, or 32 bytes).
    :param iv: The initialization vector (16 bytes).
    :return: An initialized TwofishCBC instance.
    """
    return TwofishCBC(key, iv)