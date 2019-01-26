<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body>
 	<?php echo '<p>Hello, upload a file:</p>'; ?>
 
	<form action="uploadsave_image.php" method="post" 
		enctype="multipart/form-data"		>
		<label for="file">Filename:</label>
		<input type="file" name="file" id="file" />
		<br />
		<input type="submit" name="submit" value="submit" />
	</form> 
 </body>
</html>