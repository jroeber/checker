# Examples

## Configuration

Checker is designed to be simple. Configuration is done via a YAML file with the following structure:

```yaml
hosts:
  - host: somesite.com
  - host: anothersite.com
```

Each `host` line contains a *socket address*, which may or may not include a port number. If the port number isn't included, Checker will just ping that host to see if it's up. If the port number *is* included, Checker will try to open a TCP connection, or, if it recognizes the port (e.g. port 80/HTTP), it will use the defaults for the protocol most commonly associated with that port:

```yaml
hosts:
  - host: simple.example # will just ping this host
  - host: niche-service.example:1234 # will open a TCP connection on port 1234 of this host
  - host: normal-web-server.example:80 # will send HTTP request and expect "200 OK" response
```

The `host` line can also have other options associated with it: `rate`, `timeout`, and `type`.

- `rate` determines how often the check will be made in seconds. Default is 15.
- `timeout` determines how long the check will wait before reporting a bad status. Default is 15.
- `type` determines the type of check to be made, if the default isn't good enough. Currently, the only types supported are `PING`, `TCP`, and `HTTP`.

```yaml
hosts:
  - host: mysite.example
    type: http # will cause the checker to treat this entry as if it had :80 as its port
  - host: slowdown.example:443
    rate: 30 # check every 30 seconds
    timeout: 60 # wait 60 seconds for the connection to be established before reporting a bad status
```

If you want to expect a different HTTP status code, you can use the `expectedCode` option:

```yaml
hosts:
  - host: should-have-nothing.example:80
    expectedCode: 404 # will report bad status if it receives a response that *isn't* 404
```

## Prometheus Metrics

Checker can act as a Prometheus endpoint, allowing a Prometheus server to scrape metrics from it and then be visualized with e.g. Grafana. This is enabled by default on port 8080. You can specify a different port with `--prometheus-port` or disable the endpoint with `--no-prometheus`.

## Running

Running the program is easy. You just need the `checker` binary and a config file:

```bash
checker -c checks.yaml # with console visualization
checker -c checks.yaml --no-console # without console visualization
```

It's possible to run the program with metrics disabled *and* no console visualization; in this case, the checks are still made, but there's no feedback about status. That's pointless, but you're free to do it if you feel like.

### Docker

You can also run via Docker. The image expects to find `config.yml` at the root directory, so bind-mount that in. An example Docker run:

```bash
docker run \
  -d \
  -v $(pwd)/config.yml:/config.yml \
  -p 8080:8080 \
  --restart unless-stopped \
  --name checker \
  jroeber/checker
```
