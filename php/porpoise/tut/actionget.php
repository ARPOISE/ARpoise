<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body>
 Hi <?php echo htmlspecialchars($_GET['name']); ?>.
 You are <?php echo (int)$_GET['age']; ?> years old.  
 </body>
</html>