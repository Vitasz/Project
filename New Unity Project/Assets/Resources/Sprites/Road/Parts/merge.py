import sys
from PIL import Image

def merge(a,b):
    new_im = Image.alpha_composite(a, b)
    return new_im
#now1 = 0
now2 = 0
#image1 = Image.new('RGBA',(100,100))
#image1 = Image.alpha_composite(image1, Image.open('101000000.png').convert('RGBA'))
#merge(image1, image2).save(name)
imagestart = Image.open("start.png").convert('RGBA')
left = Image.open("7.png").convert('RGBA')
up = Image.open("1.png").convert('RGBA')
now = 0

while(now<16):
    nowstr = str(bin(now)[2::])
    while((len)nowstr<4)nowstr=nowstr+'0'
    if (nowstr.count('1')==2):
        
    else:
        
    
