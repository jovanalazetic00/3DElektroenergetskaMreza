   M S B A M L   `   `   `  ����>  :PZ2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:PZ2PZ2  5   	       PZ2.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   #localclr-namespace:PZ2     5   	     � WindowState$  	Maximized=�5   	    � Title$	 PZ2��5	   	   $	��600��6   $
��1000��6"   
����İ�.��5
      � .+�5   
   '� -   5         '�Name$ viewPort��5      x�5      g� 5       g� Content 5      t� 5      5      g� 5       5      h� 5      -   5        
MyView��.��5      � -   5        Map�� � Material 5   "   u� 5   &    u� Brush 5   *   �� 5   .    �� ImageSource$ MapImage/map.jpg��69   5   '    � Geometry 5   "   k� 5    &    k� 	Positions �   	 k� TextureCoordinates	 �   
 k� TriangleIndices
 �     6�   5"       j� 	Transform 5#      Y� 5$   "   .�5%   &   S� -   5        transliranje�� S� OffsetX$ 0Q�5%   Q    S� OffsetY$ 0Q�6]    S� OffsetZ$ 0Q�6i   �� 5&   &   -   5        
skaliranje�� �� ScaleX$ 1Q�5&   K    �� ScaleY$ 1Q�6V    �� ScaleZ$ 1Q�6a   �� 5'   &    �� CenterX$
 0.25Q�68    �� CenterY$
 0.25Q�6G    �� Rotation 5(   *   �� 5)   .   -   5        myAngleRotationX�� �� Axis$ 1,0,05�5)   \   5+   '   �� 5,   &    5-   *   �� 5.   .   -   5        myAngleRotationY��$ 0,0,15�5.   \   50   '   51   #   53      55      57      �� 5:   
   
�������$��1q�6%   $	��338��69   $	��266��6F   $	��Top=�5;      $
��Left=�6)   
�������._�5=      �� $��	0,0,0,-88q�6   .+�5@      	� -   5         	�Name$ 
cbR01color��5@   A   $��	10,15,0,0q�6    	� 	GroupName$	 Rs2��61   
���   � J� RenderTransformOrigin$ 0.504,1.246&�6�   $��Oboji vodove sa R-> 0 - 1��6�    d� 	IsChecked 6h   �� *CheckedR01color+6�   	� 5A      -	   5        $ 
cbR12color��5A   @   $��10,5,0,0q�6   $	 Rs2��60   
���   �$ 0.504,1.246&�6�   $��Oboji vodove sa R-> 1 - 2��6�    6g   �� *CheckedR12color+6�   	� 5B      -
   5        $ 	cbR3color��5B   @   $��10,5,0,0q�6   $	 Rs2��60   
���   �$ 0.504,1.246&�6�   $!��Oboji vodove sa R-> preko 2��6�    6f   �� *CheckedR3color+6�   	� 5C      -   5        $ cbRevertLineColor��5C   @   $��10,5,0,0q�6   $	 Rs2��60   
���   �$ 0.504,1.246&�6�   $*��$Vrati boje vodova u prvobitno stanje��6�    6n   �� *CheckedRevertLineColor+6�   	� 5E      -   5        $ cbswitchcolor��5E   A   $��	10,15,0,0q�6   $	 Rs3��61   
���   �$ 0.504,1.246&�6�   $,��&Prikaži Otvorene/Zatvorene prekidače��6�    6k   �� *CheckedSwitchStatus+6�   	� 5F      -   5        $ cbrevertswitch��5F   @   $��10,5,0,0q�6   $	 Rs3��60   
���   �$ 0.504,1.246&�6�   $)��#Vrati prekidače u prvobitno stanje��6�    6k   �� *RevertSwitchStatus+6�   	� 5H      -   5        $ 
cbOpenHide��5H   A   $��	10,15,0,0q�6   $	 Rs4��61   
���   �$ 0.504,1.246&�6�   $��Sakrij neaktivan dio��6�    6h   �� *CheckedOpen+6�   	� 5I      -   5        $ 
cbOpenShow��5I   @   $��10,5,0,0q�6   $	 Rs4��60   
���   �$ 0.504,1.246&�6�   $%��Vrati mrežu u prvobitno stanje��6�    6g   �� *CheckedOpen+6�   5K      5L      5Q      5R      