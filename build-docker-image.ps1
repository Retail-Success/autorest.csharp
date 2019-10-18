param(
    [String] $Version
)

#repository must be created already and you must have already logged in via docker (docker login)

docker build -t retailsuccess/autorest:latest .

docker tag retailsuccess/autorest:latest retailsuccess/autorest:$Version

docker push retailsuccess/autorest:latest

docker push retailsuccess/autorest:$Version