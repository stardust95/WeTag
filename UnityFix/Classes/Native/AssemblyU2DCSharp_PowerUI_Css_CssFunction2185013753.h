﻿#pragma once

#include "il2cpp-config.h"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif

#include <stdint.h>

#include "mscorlib_System_Object2689449295.h"

// System.String
struct String_t;




#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

// PowerUI.Css.CssFunction
struct  CssFunction_t2185013753  : public Il2CppObject
{
public:
	// System.String PowerUI.Css.CssFunction::Name
	String_t* ___Name_0;

public:
	inline static int32_t get_offset_of_Name_0() { return static_cast<int32_t>(offsetof(CssFunction_t2185013753, ___Name_0)); }
	inline String_t* get_Name_0() const { return ___Name_0; }
	inline String_t** get_address_of_Name_0() { return &___Name_0; }
	inline void set_Name_0(String_t* value)
	{
		___Name_0 = value;
		Il2CppCodeGenWriteBarrier(&___Name_0, value);
	}
};

#ifdef __clang__
#pragma clang diagnostic pop
#endif