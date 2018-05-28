#ifndef __N_DEFINES_H__
#define __N_DEFINES_H__

#ifdef _MSC_VER
#define EXPORT __declspec(dllexport)
#elif defined(__GNUC__)
#define EXPORT __attribute__((visibility("default")))
#else
#define EXPORT
#endif

#endif