import sys
import googlevoice
import os.path
from googlevoice import Voice
from googlevoice.util import input

print "attempting log in..."

voice = Voice()
voice.login("mikepreble@gmail.com", "sukirevomnrhqxxo")

print "login successful!"

print str(sys.argv)
print len(sys.argv)

errors = 0

if len(sys.argv) == 3:
    try:
        text = sys.argv[1]
        path = (sys.argv[2])
        file = open(path)
        numbers = file.read()
        numbers = numbers.split('\n')
        print str(numbers)
        i = 0
        while i < len(numbers):
            print "sending message to " + numbers[i]
            voice.send_sms(numbers[i], text)
            
            i += 1
    except IOError as e:
        errors = errors + 1
        print e
        
print "" + str(errors) + " errors"
print "Done!"


        
    #voice.send_sms(phoneNumber, text)

#phoneNumber = input('Number to send message to: ')
#text = input('Message text: ')

#input("press any key to continue...")
