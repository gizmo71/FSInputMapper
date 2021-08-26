#include <Bounce2.h>
#include <IoAbstraction.h>

class IoBounce : public Bounce {
  IoAbstractionRef ioAbstraction;
public:
  IoBounce(IoAbstractionRef ioAbstraction) : Bounce(), ioAbstraction(ioAbstraction) { }
  bool readCurrentState() { return ioAbstraction->readValue(pin); }
  void setPinMode(int pin, int mode) { ioAbstraction->pinDirection(pin, mode); }
};
