#r ".\Chapter13.exe"

using Chapter13;

Converter<object, string> converter = x => x.ToString();
Converter<string, string> contravariance = converter;
Converter<object, object> covariance = converter;
Converter<string, object> both = converter;
