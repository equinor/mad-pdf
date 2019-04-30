
# ASP.NET Core / NodeServices / Puppeteer / Imagemagick / Ghostscript

* Convert html to pdf
* Convert pdf to png


## To run

```
cd src
dotnet run
```

## To run in Docker

```
cd src
docker build -f Dockerfile .
```

Then run the image as a container using

```
docker run -d -p 8000:80 madpdf
```

## pdf2png
**Optional**
page (default = 0)
dpi (default 180, range between 1 - 300)
```
curl -F 'file=@test.pdf' https://example.azurewebsites.net/api/v1/pdf/pdf2png?page=0&dpi=180 > test.png && open test.png
```


## html2pdf
```
curl -d "{ html: '<h1>header </h1> <p>123</p>', config: { format: "A4", landscape: false } }" -H "Content-Type: application/json" -X POST https://example.azurewebsites.net/api/v1/pdf/html2pdf > test.pdf && open test.pdf
```