<html>
 <head>
  <title>Upload action file</title>
 </head>
 <body>
 <?php echo '<p>Hello, upload an image file</p>'; ?>
 

 <?php
 // For IE to recognize jpg files the type must be pjpeg, for FireFox it must be jpeg.
 // Restrict file size to less that 20 kb
  if ((
  		($_FILES["file"]["type"] == "image/png")
  	 || ($_FILES["file"]["type"] == "image/jpeg")
  	 || ($_FILES["file"]["type"] == "image/pjpeg"))
  	 && ($_FILES["file"]["size"] <  20000 ))   		
  {
 	if ($_FILES["file"]["error"] > 0 )
 		{ echo "Error: " . $_FILES["file"]["error"] . "<br/>";
 		}
 	else
 	{
 		echo "Upload:    " . $_FILES["file"]["name"] . "<br/>";
 		echo "Type:      " . $_FILES["file"]["type"] . "<br/>";
 		echo "Size:      " . $_FILES["file"]["size"] / 1024 . "KB<br/>";
 		echo "Temp file:  " . $_FILES["file"]["tmp_name"] . "<br/>";
 		
 		if (file_exists("upload/" . $_FILES["file"]["name"]))
 			{ echo $_FILES["file"]["name"] . "already exists. "; }
 		else { move_uploaded_file($_FILES["file"]["tmp_name"],
 				"upload/" . $_FILES["file"]["name"]);
 				echo "Stored in: " . "upload/" . $_FILES["file"]["name"] ; 
  		}
 	}
  }
  else { echo "Invalid file - must be png or jpg under 20KB"; }
 ?>
 </body>
</html>

 		