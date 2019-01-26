<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body>
 <?php echo '<p>Hello World</p>'; ?>
 <?php 
 	if (strpos($_SERVER['HTTP_USER_AGENT'], 'MSIE') !== FALSE) {
 ?>
 	<h1>IE</h1>
 <?php
 } else {
 ?>
 <H1>not IE</H1>
 <?php
 }
 ?>  
  
 </body>
</html>