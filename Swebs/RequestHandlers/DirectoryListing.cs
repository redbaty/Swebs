﻿using NHttp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swebs.RequestHandlers
{
	public class DirectoryListing : IRequestHandler
	{
		public string _rootPath;

		public DirectoryListing(string rootPath)
		{
			_rootPath = rootPath;
		}

		public void Handle(HttpRequestEventArgs args, string requestPath, string localPath)
		{
			var request = args.Request;
			var response = args.Response;

			response.ContentType = "text/html";

			var list = this.RenderDirectoryList(requestPath, localPath);
			using (var writer = new StreamWriter(response.OutputStream))
				writer.Write(list);
		}

		private string RenderDirectoryList(string requestPath, string localPath)
		{
			var directoryName = "/" + requestPath;
			if (string.IsNullOrWhiteSpace(directoryName))
				directoryName = "/";

			var backPath = "/";
			if (!string.IsNullOrWhiteSpace(requestPath))
				backPath += Path.GetDirectoryName(requestPath).Replace(_rootPath, "");

			var sb = new StringBuilder();

			sb.AppendLine("<!DOCTYPE html>");
			sb.AppendLine("<html>");
			sb.AppendLine("<head>");
			sb.AppendLine("<title>Index of " + directoryName + "</title>");
			sb.AppendLine("</head>");
			sb.AppendLine("<body>");

			{
				sb.AppendFormat("<h1>Index of {0}</h1>", directoryName);

				sb.AppendLine("<table>");

				sb.AppendLine("<tr>");
				sb.AppendFormat("<th>Name</th>");
				sb.AppendLine("<th>Last modified</th>");
				sb.AppendLine("<th>Size</th>");
				sb.AppendLine("</tr>");

				sb.AppendLine("<tr>");
				sb.AppendLine("<tr><th colspan=\"3\"><hr></th></tr>");
				sb.AppendLine("</tr>");

				sb.AppendLine("<tr>");
				sb.AppendFormat("<td><a href='{0}'>Parent Directory</a></td>", backPath);
				sb.AppendLine("<td></td>");
				sb.AppendLine("<td></td>");
				sb.AppendLine("</tr>");

				foreach (var filePath in Directory.EnumerateDirectories(localPath, "*", SearchOption.TopDirectoryOnly))
				{
					var name = Path.GetFileName(filePath);
					var linkPath = filePath.Replace(_rootPath, "");
					linkPath = linkPath.NormalizePath();
					if (!linkPath.StartsWith("/"))
						linkPath = "/" + linkPath;

					sb.AppendLine("<tr>");
					sb.AppendFormat("<td>[D] <a href='{0}'>{1}</a></td>", linkPath, name);
					sb.AppendFormat("<td>{0:yyyy-MM-dd HH:mm:ss}</td>", Directory.GetLastWriteTime(filePath));
					sb.AppendFormat("<td>-</td>");
					sb.AppendLine("</tr>");
				}

				foreach (var filePath in Directory.EnumerateFiles(localPath, "*", SearchOption.TopDirectoryOnly))
				{
					var name = Path.GetFileName(filePath);
					var linkPath = filePath.Replace(_rootPath, "");
					linkPath = linkPath.NormalizePath();
					if (!linkPath.StartsWith("/"))
						linkPath = "/" + linkPath;

					sb.AppendLine("<tr>");
					sb.AppendFormat("<td>[F] <a href='{0}'>{1}</a></td>", linkPath, name);
					sb.AppendFormat("<td>{0:yyyy-MM-dd HH:mm:ss}</td>", File.GetLastWriteTime(filePath));
					sb.AppendFormat("<td>{0}</td>", this.GetSizeString(filePath));
					sb.AppendLine("</tr>");
				}

				sb.AppendLine("<tr>");
				sb.AppendLine("<tr><th colspan=\"3\"><hr></th></tr>");
				sb.AppendLine("</tr>");

				sb.AppendLine("</table>");
			}

			sb.AppendLine("</body>");
			sb.AppendLine("</html>");

			return sb.ToString();
		}

		private string GetSizeString(string filePath)
		{
			var result = new FileInfo(filePath).Length;
			if (result < 1024)
				return result + "B";

			var resultf = result / 1024f;
			if (resultf < 1024)
				return resultf.ToString("0.##", CultureInfo.InvariantCulture) + "K";

			resultf /= 1024;
			if (resultf < 1024)
				return resultf.ToString("0.##", CultureInfo.InvariantCulture) + "M";

			resultf /= 1024;
			return result.ToString("0.##", CultureInfo.InvariantCulture) + "G";
		}
	}
}