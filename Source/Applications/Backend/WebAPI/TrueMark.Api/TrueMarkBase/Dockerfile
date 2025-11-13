FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80

ARG temp_dir="/tmp/cprocsp_packages"
RUN mkdir -p $temp_dir
# необходимо скачать со страницы https://www.cryptopro.ru/products/csp/downloads
COPY ["Applications/Backend/WebAPI/TrueMark.Api/CryptoProCspLocalPackages/Distrib/linux-amd64_deb.tgz", "$temp_dir"]

RUN cd "$temp_dir" && \
    tar -xf linux-amd64_deb.tgz && \
	linux-amd64_deb/install.sh && \
	rm -rf "$temp_dir"