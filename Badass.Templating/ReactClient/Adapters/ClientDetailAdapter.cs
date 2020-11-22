﻿using System;
using System.Collections.Generic;
using System.Linq;
using Badass.Model;

namespace Badass.Templating.ReactClient.Adapters
{
    public class ClientDetailAdapter : ClientApiAdapter
    {
        public ClientDetailAdapter(SimpleType type, Domain domain) : base(type, domain)
        {
        }

        public List<ClientApiAdapter> RelatedDetails // LinkedDetails + DirectlyRelatedDetails
        {
            get
            {
                var related = new List<ClientApiAdapter>(DirectlyRelatedDetails);
                foreach (var item in LinkedDetails)
                {
                    if (related.All(r => r.Name != item.Name))
                    {
                        related.Add(item);
                    }
                }

                return related;
            }
        } 

        public List<LinkingApiAdapter> LinkedDetails
        {
            get
            {
                var types = _domain.Types.Where(t =>
                    t.Fields.Any(f => f.HasReferenceType && f.ReferencesType == _type));

                var linkTypes = new List<LinkingApiAdapter>();
                foreach (var link in types.Where(t => t.IsLink))
                {
                    var otherSideOfLink = link.Fields.Where(f => f.HasReferenceType && f.ReferencesType != _type && !f.ReferencesType.IsSecurityPrincipal).Select(f => f.ReferencesType);
                    if (otherSideOfLink.Count() > 1)
                    {
                        Console.WriteLine("Warning: link type links to multiple 'other' things"); // templates have not bee designed for this
                    }
                    else
                    {
                        if (otherSideOfLink.Any())
                        {
                            linkTypes.Add(new LinkingApiAdapter(otherSideOfLink.First(), _domain, link));
                        }
                    }
                }

                return linkTypes.OrderBy(l => l.LinkingType.Name).ToList();
            }
        }

        public List<LinkByFieldClientApiAdapter> DirectlyRelatedDetails
        {
            get
            {
                var types = _domain.Types.Where(t =>
                    t.Fields.Any(f => f.HasReferenceType && f.ReferencesType == _type) && !t.IsLink && t != (ApplicationType)this._type).OrderBy(t => t.Name);

                return types.Select(t =>
                        new LinkByFieldClientApiAdapter(t, _domain,
                            t.Fields.First(f => f.ReferencesType == this._type)))
                    .ToList();
            }
        }


    }
}
