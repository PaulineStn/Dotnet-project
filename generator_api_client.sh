export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$PATH:$HOME/.dotnet"
export PATH="$PATH:$HOME/.dotnet/tools"

nswag openapi2csclient \
  /input:http://localhost:5231/openapi/v1.json \
  /output:Gauniv.Network/ApiClient.cs \
  /classname:ApiClient \
  /namespace:Gauniv.Network