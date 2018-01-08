openssl genrsa -out openssl.key 1024

openssl req -new -x509 -key openssl.key -out openssl.cer -days 3650 -subj /CN=localhost

openssl pkcs12 -export -out openssl.pfx -inkey openssl.key -in openssl.cer
