
#pragma once

#include <Shell3D/Shell3DTypeIds.h>

#include <AzCore/EBus/EBus.h>
#include <AzCore/Interface/Interface.h>

namespace Shell3D
{
    class Shell3DRequests
    {
    public:
        AZ_RTTI(Shell3DRequests, Shell3DRequestsTypeId);
        virtual ~Shell3DRequests() = default;
        // Put your public methods here
    };

    class Shell3DBusTraits
        : public AZ::EBusTraits
    {
    public:
        //////////////////////////////////////////////////////////////////////////
        // EBusTraits overrides
        static constexpr AZ::EBusHandlerPolicy HandlerPolicy = AZ::EBusHandlerPolicy::Single;
        static constexpr AZ::EBusAddressPolicy AddressPolicy = AZ::EBusAddressPolicy::Single;
        //////////////////////////////////////////////////////////////////////////
    };

    using Shell3DRequestBus = AZ::EBus<Shell3DRequests, Shell3DBusTraits>;
    using Shell3DInterface = AZ::Interface<Shell3DRequests>;

} // namespace Shell3D
