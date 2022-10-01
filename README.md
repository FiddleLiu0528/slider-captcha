# slider-captcha
use core6 cross platform imageshark generate slider captcha image for front-end users validate

# use package 
https://www.nuget.org/packages/SixLabors.ImageSharp.Drawing/
NuGet\Install-Package SixLabors.ImageSharp.Drawing -Version 1.0.0-beta15

# todo 
1. add front-end parse slider tool
2. add back-end parse api
3. add mobile device Support
4. add dockerfile
5. add i18n/

# picture
picture come from: [https://picsum.photos/]

# dockerfile
1. docker build -t web-app01 .
2. docker run -d -p 8881:80 --name myapp01 web-app01