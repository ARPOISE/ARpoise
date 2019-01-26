<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body>
 <?php echo '<p>Hello World</p>'; ?>
 <?php 
 	if (strpos($_SERVER['HTTP_USER_AGENT'], 'MSIE') !== FALSE) {
 		echo 'IE <br /> wohl';
 	}
 ?>
 </body>
</html>