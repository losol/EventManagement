# Updating docker image

in / folder

```bash
docker build -t eventmanagement .
docker tag eventmanagement losolio/eventmanagement
docker login
docker push losolio/eventmanagement
```