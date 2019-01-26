<html>
 <head>
  <title>Upload action file</title>
 </head>
 <body>
 <?php echo '<p>Hello upload action file</p>'; ?>
 <?php 
 	if ($_FILES["file"]["error"] > 0 )
 		{ echo "Error: " . $_FILES["file"]["error"] . "<br/>";
 		}
 	else
 	{
 		echo "Upload:    " . $_FILES["file"]["name"] . "<br/>";
 		echo "Type:      " . $_FILES["file"]["type"] . "<br/>";
 		echo "Size:      " . $_FILES["file"]["size"] / 1024 . "<br/>";
 		echo "Stored in: " . $_FILES["file"]["tmp_name"];
 	}
 ?>
 </body>
</html>