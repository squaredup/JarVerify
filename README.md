<img src="https://img.shields.io/nuget/v/JarVerify.svg"/>&nbsp;<img src="https://ci.appveyor.com/api/projects/status/u1of0xu7724cfyh2?svg=true"/>

# JarVerify
Verify the digital signatures of signed JARs (or just any ZIPs signed by `jarsigner`)

## Dependencies
Requires BouncyCastle.Crypto for PKCS7 verification

## Intended use case
This library is designed to perform JAR verification against an **already known** certificate with a public key. The current implementation also assumes that the name of the signature file (i.e. `NAME.SF`) is known (because this is generally fixed for a given certificate)

This library cannot currently be used to verify the signature of a JAR for which you do not have the public key already (or are not prepared to supply any keys during verification)

### Why would you ever want or use this?
Since JARs are just ZIP files, the `jarsigner` and its toolchain can be used to digitally sign arbitrary ZIP files. With this library, you can then verify these JAR/ZIP files from a .NET application

#### But doesn't a signed JAR only guarantee the certificate and not the content of the JAR?
This is true - for example, although every file stated in the `MANIFEST.MF` is hashed, you can actually add additional files _after_ signing. You're not obligated to add them to the manifest, and it has no effect on the digital signature validity 

This is obviously unacceptable for use as an arbitrary ZIP signing capability

For this reason, this library has some non-standard/non-compliant features:

* As stated above - the requirement of a specific public key to validate _against_
* The count of all non-signature files in the ZIP is checked against the number of entries in the `MANIFEST.MF`. If there is a mismatch, verification immediately fails
    * Adding new files to the manifest changes the hash of the manifest file - and therefore invalidates the content of the signature file. If you modify the signature file, you completely invalidate the signature (since the PKCS signing is against the digest of the signature file)
    
## Limitations
* This library assumes that **only** SHA-256 digests are being used. Any other digest type will be ignored and validation will fail
    * Really, nothing should be using SHA1 or lower any more any way so...
* No consideration for certificate chains 
    * The anticipated use case is self-signed certificates: for this reason, the certificate chain is not explicitly dealt with and self-signed is not considered invalid
* RSA and DSA are considered, but only DSA has been tested in practice

# Example
```C#
VerificationCertificates certs = new VerificationCertificates();
certs.Add("SIGNFILE", File.ReadAllBytes("public_certificate.cer")));

var result = Verify.Jar("my.jar" certs);

// Result contains Valid and Status properties
```

Error handling in this library is currently not ideal - many common validation failures will return a result with `Valid` false. More esoteric issues (e.g. unparseable manifest file) throw exceptions. 

For this reason, an expected use might be a structure like the following (if you care _only_ about if the JAR is validly signed and do not care why the JAR is not valid)

```C#
try
{
   var result = Verify.Jar("my.jar" certs);
   
   return result.Valid;
}
catch
{
    return false;
}
```

Obviously if you want actual diagnostic information about _why_ the JAR is invalid, the exception details should be dumped to a log.
