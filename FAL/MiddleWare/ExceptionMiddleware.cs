using Amazon.Rekognition.Model;
using Newtonsoft.Json;
using Share.ExceptionModels;

namespace FAL.MiddleWare
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("Content-Type", "application/json");
            try
            {
                await _next(context);
            }
            catch (BadRequestException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                BaseException serviceResult = new BaseException()
                {
                    ErrorCode = StatusCodes.Status400BadRequest,
                    UserMsg = ex.Message,
                    DevMessage = ex.Message,
                    TraceId = context.TraceIdentifier,
                    MoreInfo = ex.HelpLink,
                };
                var res = JsonConvert.SerializeObject(serviceResult);
                await context.Response.WriteAsync(res);
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                BaseException serviceResult = new BaseException()
                {
                    ErrorCode = StatusCodes.Status404NotFound,
                    UserMsg = ex.Message,
                    DevMessage = ex.Message,
                    TraceId = context.TraceIdentifier,
                    MoreInfo = ex.HelpLink,
                };
                var res = JsonConvert.SerializeObject(serviceResult);
                await context.Response.WriteAsync(res);
            }
            catch (ConflictException ex)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                BaseException serviceResult = new BaseException()
                {
                    ErrorCode = StatusCodes.Status409Conflict,
                    UserMsg = ex.Message,
                    DevMessage = ex.Message,
                    TraceId = context.TraceIdentifier,
                    MoreInfo = ex.HelpLink,
                };
                var res = JsonConvert.SerializeObject(serviceResult);
                await context.Response.WriteAsync(res);
            }
            catch (InternalServerException ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                BaseException serviceResult = new BaseException()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    DevMessage = ex.Message,
                    UserMsg = ex.Message,
                };
                var res = JsonConvert.SerializeObject(serviceResult);
                await context.Response.WriteAsync(res);
            }
            //catch (InvalidJwtException ex)
            //{
            //    context.Response.StatusCode = StatusCodes.Status400BadRequest;
            //    BaseException serviceResult = new BaseException()
            //    {
            //        ErrorCode = StatusCodes.Status400BadRequest,
            //        DevMessage = ex.Message,
            //        UserMsg = "Invalid Google cridential token.",
            //    };
            //    var res = JsonConvert.SerializeObject(serviceResult);
            //    await context.Response.WriteAsync(res);
            //}
            catch (ForbiddenException ex)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                BaseException serviceResult = new BaseException()
                {
                    ErrorCode = StatusCodes.Status403Forbidden,
                    UserMsg = ex.Message,
                    DevMessage = ex.Message,
                    TraceId = context.TraceIdentifier,
                    MoreInfo = ex.HelpLink,
                };
                var res = JsonConvert.SerializeObject(serviceResult);
                await context.Response.WriteAsync(res);
            }
            catch (UnauthorizedException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                BaseException serviceResult = new BaseException()
                {
                    ErrorCode = StatusCodes.Status401Unauthorized,
                    UserMsg = ex.Message,
                    DevMessage = ex.Message,
                    TraceId = context.TraceIdentifier,
                    MoreInfo = ex.HelpLink,
                };
                var res = JsonConvert.SerializeObject(serviceResult);
                await context.Response.WriteAsync(res);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                BaseException serviceResult = new BaseException()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    UserMsg = ex.Message,
                    DevMessage = ex.Message,
                    TraceId = context.TraceIdentifier,
                    MoreInfo = ex.HelpLink,
                };
                var res = JsonConvert.SerializeObject(serviceResult);
                await context.Response.WriteAsync(res);
            }

        }
    }
}
