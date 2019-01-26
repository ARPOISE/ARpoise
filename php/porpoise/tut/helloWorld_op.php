<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body>
 <?php echo '<p>Hello World</p>'; 
 
 // debug print out
 $filename = '/srv/www/vhosts/default/htdocs/junaio/php_output.txt';
 if (file_exists($filename)) {
 	$fp = fopen($filename, 'a');
 	fwrite($fp, "\n poi output\n");
 	fwrite($fp, "<p>Hello World too</p>");
 	fwrite($fp, "\n");
 	fclose($fp);
 }
 
 
 ?>
 
 
 
 <?php phpinfo(); ?>
 </body>
</html>