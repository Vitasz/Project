import sys
from PIL import Image

def merge(a,b,c):
    new_im1 = Image.alpha_composite(b, c)
    new_im = Image.alpha_composite(a, new_im1)
    return new_im
#merge("0000000000000000.png", "0000000000000001.png", "0000000000010000.png")
now1 = 0
now2 = 0
while(now1<16):
    while now2<16:
        name1 = str(bin(now1))[2::]
        while(len(name1)<4):
            name1='0'+name1
        name2 = str(bin(now2))[2::]
        while(len(name2)<4):
            name2='0'+name2
        ok = True
        for i in range(len(name1)):
            if (name1[i]=='1' and name1[i]==name2[i]):
                ok = False
                break
        if (ok and (name1!='0000' or name2!='0000')):
            image1 = Image.new('RGBA',(100,100))
            if (name1[0]=='1'):
                image1 = Image.alpha_composite(image1, Image.open('101000000.png').convert('RGBA'))
            if (name1[1]=='1'):
                image1 = Image.alpha_composite(image1, Image.open('100010000.png').convert('RGBA'))
            if (name1[2]=='1'):
                image1 = Image.alpha_composite(image1, Image.open('100000100.png').convert('RGBA'))
            if (name1[3]=='1'):
                image1 = Image.alpha_composite(image1, Image.open('100000001.png').convert('RGBA'))

            image2 = Image.new('RGBA',(100,100))
            if (name2[0]=='1'):
                image2 = Image.alpha_composite(image2, Image.open('201000000.png').convert('RGBA'))
            if (name2[1]=='1'):
                image2 = Image.alpha_composite(image2, Image.open('200010000.png').convert('RGBA'))
            if (name2[2]=='1'):
                image2 = Image.alpha_composite(image2, Image.open('200000100.png').convert('RGBA'))
            if (name2[3]=='1'):
                image2 = Image.alpha_composite(image2, Image.open('200000001.png').convert('RGBA'))
            name = ''
            for i in name1+name2:
                name+='0'+i
            name+='.png'
            merge(Image.open("0000000000000000.png").convert('RGBA'), image1, image2).save(name)

        now2+=1
    now1+=1
    now2= 0

        
