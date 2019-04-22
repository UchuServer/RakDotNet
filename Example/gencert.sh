#!/bin/bash

openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes -subj '/CN=localhost'
openssl pkcs12 -export -out cert.p12 -in cert.pem -inkey key.pem -password pass:1234
rm -v {key,cert}.pem
