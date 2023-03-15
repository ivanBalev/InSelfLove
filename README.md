<h1 align="center">
    <a href="https://www.inselflove.com"><img src="https://www.inselflove.com/Custom/icons/flower-green.svg" alt="Markdownify" width="100"></a>
  <br>
    InSelfLove
</h1>
<div align="center">
  <img src="https://github.com/devicons/devicon/blob/master/icons/mysql/mysql-original-wordmark.svg" title="MySQL" alt="MySQL" width="40" height="40"/>&nbsp;
  <img src="https://github.com/devicons/devicon/blob/master/icons/dotnetcore/dotnetcore-original.svg" title=".NET" alt=".NET" width="40" height="40"/>&nbsp;
  <img src="https://github.com/devicons/devicon/blob/master/icons/javascript/javascript-original.svg" title="JavaScript" alt="JavaScript" width="40" height="40"/>&nbsp;
  <img src="https://github.com/devicons/devicon/blob/master/icons/bootstrap/bootstrap-original-wordmark.svg" title="Bootstrap" alt="Bootstrap" width="40" height="40"/>&nbsp;
</div>

<h4 align="center"> Content management system for <a href="https://www.instagram.com/in.self.love">Mimi Marinova</a>’s psychotherapy practice</h4>

<h4 align="center">You can find a dummy version of the app at <a href="https://test.inselflove.com">test.inselflove.com</a></h4>

<div align="center">
 <table>
  <tr align="center">
    <td>Username</td>
     <td>Password</td>
  </tr>
  <tr align="center">
    <td valign="top">admin</td>
    <td valign="top">adminadmin</td>
  </tr>
    <tr align="center">
    <td valign="top">user</td>
    <td valign="top">useruser</td>
  </tr>
 </table>
</div>

## Key Features

* Improved initial load times
  - <b>Cloudinary</b> provides optimized image size and format
  - <b>lite-youtube-embed</b> renders embedded youtube videos significantly faster
  - <b>Bundling and minification</b> of JS and CSS reduce requests' number and size
  <p>
  <img src="https://res.cloudinary.com/dzcajpx0y/image/upload/v1678834424/Lighthouse_gaiiug.png" alt="LighthouseScore" width="500">
  </p>
  
* Server-side and client-side rendering
  - Provides rich site interaction with good SEO
  - Comments and search pagination make use this approach
   
* reCAPTCHA Enterprise implemented to prevent abusive traffic without user friction

* Stripe payment integrated into project UI
  - Test it out by logging in as admin and creating an appointment slot by clicking on any day in the calendar
  - Then, as user, reqeust to book the slot
  - As admin, approve the request
  - Once approved, the user will be given the option to pay
  - 4242 4242 4242 4242 - valid card
  - 4100 0000 0000 0019 - invalid card
  - any future date under 'Expiration' and any CVC works
<p>
   <img src="https://res.cloudinary.com/dzcajpx0y/image/upload/v1678906628/aididie_fvo2r9.gif" title="StripePayment" alt="StripePayment" width="740"/>&nbsp;
</p>
  
* Attention to detail for improved UX
  - The <b>EnterContentSyllables</b> method in <b>ArticlesController</b> provides an alternative to 'text-align: justify'
  - This approach produces a more natural reading experience and makes text more compact
  - Below you can see default | text-align: justify | syllabified content
 <p>
 <img src="https://res.cloudinary.com/dzcajpx0y/image/upload/v1678839130/preview_1_npkj1c.png" width="32%">
 <img src="https://res.cloudinary.com/dzcajpx0y/image/upload/v1678838888/preview_3_buqh3l.png" width="32%">
 <img src="https://res.cloudinary.com/dzcajpx0y/image/upload/v1678836876/preview_2_ix97bf.png" width="32%" align="top">
 </p>
 

* Extensive unit testing of services and integration testing
<div>
   <img src="https://github.com/devicons/devicon/blob/master/icons/selenium/selenium-original.svg" title="Selenium" alt="Selenium" height="40"/>&nbsp;
   <img src="https://blog.qa-services.dev/wp-content/uploads/2020/03/xUnitLogo-e1584384960711.png" title="xUnit" alt="xUnit" height="40"/>&nbsp;
</div>
<p>
   <img src="https://res.cloudinary.com/dzcajpx0y/image/upload/v1678876435/TestsCoverage_jyz7as.png" title="TestCoverage" alt="TestCoverage" width="900"/>&nbsp;
</p>
