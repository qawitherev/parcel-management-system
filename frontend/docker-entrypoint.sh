# Get environment value 
BACKEND_HOST=${BACKEND_HOST:-localhost}
BACKEND_PORT=${BACKEND_PORT:-5163}
echo "Configuration:"
echo "BACKEND_HOST: $BACKEND_HOST"
echo "BACKEND_PORT: $BACKEND_PORT"

# Replace value inside nginx.conf (the placeholders)
echo "Replacing placeholder with environment values"
envsubst '${BACKEND_HOST} ${BACKEND_PORT}' \
< /etc/nginx/nginx.conf.template \
> /etc/nginx/nginx.conf

# test nginx config
echo "Testing nginx"
nginx -t 

# start nginx
echo "Passing control to nginx. Starting NGINX..."
exec nginx -g 'daemon off;'
