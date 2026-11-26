#!/bin/sh
set -e

export PORT=${PORT:-10000}

envsubst '$PORT' < /etc/gateway/nginx.conf.template > /etc/nginx/nginx.conf

exec supervisord -c /etc/gateway/supervisord.conf